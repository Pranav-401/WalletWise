using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using WalletWise.Models;

namespace WalletWise.Data.Service
{
    public class AIInsightsService : IAIInsightsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AIInsightsService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<AIInsightsViewModel> GenerateInsightsAsync(
            IEnumerable<Expense> expenses,
            CancellationToken cancellationToken = default)
        {
            var expenseList = expenses.ToList();

            if (!expenseList.Any())
            {
                return new AIInsightsViewModel
                {
                    Summary = "Start recording your expenses to receive personalized financial insights.",
                    Improvements = new List<AIImprovement>
                    {
                        new AIImprovement
                        {
                            Title = "Build your expense tracking habit",
                            Steps = new List<string>
                            {
                                "Record your daily expenses.",
                                "Assign each expense to a category.",
                                "Review your spending regularly."
                            }
                        }
                    }
                };
            }

            var apiKey = _configuration["Groq:ApiKey"];
            var model = _configuration["Groq:Model"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "Groq API key is not configured.");
            }

            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var sixMonthsAgo = monthStart.AddMonths(-5);

            // Calculate spending for the current month.
            var currentMonthExpenses = expenseList
                .Where(e => e.Date >= monthStart && e.Date <= today.AddDays(1).AddTicks(-1))
                .ToList();

            var currentMonthTotal = currentMonthExpenses
                .Sum(e => e.Amount);

            // Calculate category-wise spending.
            var categoryTotals = currentMonthExpenses
                .GroupBy(e => e.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Total = g.Sum(e => e.Amount)
                })
                .OrderByDescending(g => g.Total)
                .ToList();

            // Calculate monthly spending for the last six months.
            var monthlyTotals = expenseList
                .Where(e => e.Date >= sixMonthsAgo && e.Date <= today)
                .GroupBy(e => new
                {
                    e.Date.Year,
                    e.Date.Month
                })
                .Select(g => new
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Total = g.Sum(e => e.Amount)
                })
                .OrderBy(g => g.Month)
                .ToList();

            // Prepare a compact summary for the AI.
            var spendingSummary = new
            {
                Currency = "INR",
                CurrentMonth = today.ToString("MMMM yyyy"),
                CurrentMonthTotal = currentMonthTotal,
                NumberOfExpenses = currentMonthExpenses.Count,
                CategoryBreakdown = categoryTotals,
                MonthlySpending = monthlyTotals
            };

            var userPrompt = $"""
                Analyze the following personal expense summary.

                {JsonSerializer.Serialize(spendingSummary)}

                Provide practical financial insights based only on this data.

                Keep observations concise.
                Give realistic improvements with 2 to 3 actionable steps.
                Do not invent missing information or assume income.
                If there is insufficient data for a comparison, say so.
                Do not give investment or loan recommendations.
                Use simple, professional language.
                """;

            var requestBody = new
            {
                model = model,
                messages = new object[]
                {
                    new
                    {
                        role = "system",
                        content = """
                            You are a professional personal finance coach.

                            Your task is to analyze a user's recorded expenses
                            and provide useful, realistic spending insights.

                            Be concise, supportive and practical.
                            Avoid long paragraphs and unnecessary explanations.
                            Never invent financial information.
                            Do not shame the user for spending.
                            Focus on improvements the user can actually implement.
                            """
                    },
                    new
                    {
                        role = "user",
                        content = userPrompt
                    }
                },
                response_format = new
                {
                    type = "json_schema",
                    json_schema = new
                    {
                        name = "financial_insights",
                        strict = true,
                        schema = new
                        {
                            type = "object",
                            properties = new
                            {
                                summary = new
                                {
                                    type = "string"
                                },
                                observations = new
                                {
                                    type = "array",
                                    items = new { type = "string" }
                                },
                                improvements = new
                                {
                                    type = "array",
                                    items = new
                                    {
                                        type = "object",
                                        properties = new
                                        {
                                            title = new
                                            {
                                                type = "string"
                                            },
                                            steps = new
                                            {
                                                type = "array",
                                                items = new { type = "string" }
                                            }
                                        },
                                        required = new[] { "title", "steps" },
                                        additionalProperties = false
                                    }
                                },
                                positiveHabits = new
                                {
                                    type = "array",
                                    items = new { type = "string" }
                                }
                            },
                            required = new[]
                            {
                                "summary",
                                "observations",
                                "improvements",
                                "positiveHabits"
                            },
                            additionalProperties = false
                        }
                    }
                }
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.groq.com/openai/v1/chat/completions");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            request.Content = JsonContent.Create(requestBody);

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Groq API request failed ({(int)response.StatusCode}): {responseBody}");
            }

            using var document = JsonDocument.Parse(responseBody);

            var content = document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException(
                    "Groq returned an empty response.");
            }

            var insights = JsonSerializer.Deserialize<AIInsightsViewModel>(
                content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return insights ?? throw new InvalidOperationException(
                "Unable to process AI insights response.");
        }
    }
}