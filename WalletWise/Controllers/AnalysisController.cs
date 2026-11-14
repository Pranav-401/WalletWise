using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WalletWise.Data.Service;


namespace WalletWise.Controllers
{
    [Authorize]
    public class AnalysisController : Controller
    {
        private readonly IExpensesService _expensesService;
        private readonly IAIInsightsService _aiInsightsService;
        private readonly ILogger<AnalysisController> _logger;

        public AnalysisController(
            IExpensesService expensesService,
            IAIInsightsService aiInsightsService,
            ILogger<AnalysisController> logger)
        {
            _expensesService = expensesService;
            _aiInsightsService = aiInsightsService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetInsights(
            CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var expenses = await _expensesService.GetAll(userId);

                var insights = await _aiInsightsService
                    .GenerateInsightsAsync(expenses, cancellationToken);

                return Json(insights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate AI financial insights.");

                Response.StatusCode = 500;

                return Json(new
                {
                    error = "AI insights could not be generated. Check the application logs."
                });
            }
        }
    }
}