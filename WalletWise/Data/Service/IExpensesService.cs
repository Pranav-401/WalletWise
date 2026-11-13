using WalletWise.Models;

namespace WalletWise.Data.Service
{
    public interface IExpensesService
    {
        Task<IEnumerable<Expense>> GetAll();
        Task Add(Expense expense);
        Task<List<ExpenseChartData>> GetChartData();
        Task<Expense?> GetById(int id);
        Task<bool> Update(Expense expense);
        Task<bool> Delete(int id);
    }
}
