using WalletWise.Models;

namespace WalletWise.Data.Service
{
    public interface IAIInsightsService
    {
        Task<AIInsightsViewModel> GenerateInsightsAsync(
            IEnumerable<Expense> expenses,
            CancellationToken cancellationToken = default);
    }
}