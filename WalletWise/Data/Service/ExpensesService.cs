using Microsoft.EntityFrameworkCore;
using WalletWise.Models;

namespace WalletWise.Data.Service
{
    public class ExpensesService : IExpensesService
    {
        private readonly FinanceAppContext _context;
        public ExpensesService(FinanceAppContext context)
        {
            _context = context;
        }
        public async Task Add(Expense expense)
        {
            _context.Expense.Add(expense);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Expense>> GetAll()
        {
            var expenses = await _context.Expense.ToListAsync();
            return expenses;
        }

        public async Task<List<ExpenseChartData>> GetChartData()
        {
            var data = await _context.Expense
                .GroupBy(e => e.Category)
                .Select(g => new ExpenseChartData
                {
                    Category = g.Key,
                    TotalAmount = g.Sum(e => e.Amount)
                })
                .ToListAsync();

            return data;
        }

        public async Task<Expense?> GetById(int id)
        {
            return await _context.Expense.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<bool> Update(Expense expense)
        {
            var existingExpense = await _context.Expense.FindAsync(expense.Id);
            if (existingExpense == null)
            {
                return false;
            }
            existingExpense.Description = expense.Description;
            existingExpense.Amount = expense.Amount;
            existingExpense.Category = expense.Category;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> Delete(int id)
        {
            var expense = await _context.Expense.FindAsync(id);
            if (expense == null)
            {
                return false;
            }
            _context.Expense.Remove(expense);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
