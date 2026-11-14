using WalletWise.Models;

namespace WalletWise.Data.Service
{
    public interface IExpensesService
    {
        Task<IEnumerable<Expense>> GetAll(string userId);
        Task Add(Expense expense);
        Task<List<ExpenseChartData>> GetChartData(string userId);
        Task<Expense?> GetById(int id, string userId);
        Task<bool> Update(Expense expense, string userId);
        Task<bool> Delete(int id, string userId);
    }
}