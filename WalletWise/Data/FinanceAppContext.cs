using Microsoft.EntityFrameworkCore;

namespace WalletWise.Data
{
    public class FinanceAppContext : DbContext
    {
        public FinanceAppContext(DbContextOptions<FinanceAppContext> options) : base(options)
        {
        }
        public DbSet<WalletWise.Models.Expense> Expense { get; set; } = null!;

    }
}
