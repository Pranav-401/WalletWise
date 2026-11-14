
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

        // Add expense for the authenticated user
        public async Task Add(Expense expense)
        {
            if (string.IsNullOrWhiteSpace(expense.UserId))
            {
                throw new ArgumentException("User ID is required.");
            }

            _context.Expense.Add(expense);
            await _context.SaveChangesAsync();
        }

        // Get only the current user's expenses
        public async Task<IEnumerable<Expense>> GetAll(string userId)
        {
            return await _context.Expense
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        // Get chart data for the current user
        public async Task<List<ExpenseChartData>> GetChartData(string userId)
        {
            return await _context.Expense
                .Where(e => e.UserId == userId)
                .GroupBy(e => e.Category)
                .Select(g => new ExpenseChartData
                {
                    Category = g.Key,
                    TotalAmount = g.Sum(e => e.Amount)
                })
                .ToListAsync();
        }

        // Get one expense belonging to the current user
        public async Task<Expense?> GetById(int id, string userId)
        {
            return await _context.Expense
                .AsNoTracking()
                .FirstOrDefaultAsync(e =>
                    e.Id == id && e.UserId == userId);
        }

        // Update only the current user's expense
        public async Task<bool> Update(Expense expense, string userId)
        {
            var existingExpense = await _context.Expense
                .FirstOrDefaultAsync(e =>
                    e.Id == expense.Id && e.UserId == userId);

            if (existingExpense == null)
            {
                return false;
            }

            existingExpense.Description = expense.Description;
            existingExpense.Amount = expense.Amount;
            existingExpense.Category = expense.Category;
            existingExpense.Date = expense.Date;

            await _context.SaveChangesAsync();
            return true;
        }

        // Delete only the current user's expense
        public async Task<bool> Delete(int id, string userId)
        {
            var expense = await _context.Expense
                .FirstOrDefaultAsync(e =>
                    e.Id == id && e.UserId == userId);

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
