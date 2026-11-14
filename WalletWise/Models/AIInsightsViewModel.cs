namespace WalletWise.Models
{
    public class AIInsightsViewModel
    {
        public string Summary { get; set; } = string.Empty;

        public List<string> Observations { get; set; } = new();

        public List<AIImprovement> Improvements { get; set; } = new();

        public List<string> PositiveHabits { get; set; } = new();
    }

    public class AIImprovement
    {
        public string Title { get; set; } = string.Empty;

        public List<string> Steps { get; set; } = new();
    }
}