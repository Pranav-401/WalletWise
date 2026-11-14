using Microsoft.EntityFrameworkCore;

namespace WalletWise.Data
{
    public class FinanceAppContext : DbContext
    {
        public FinanceAppContext(DbContextOptions<FinanceAppContext> options)
            : base(options)
        {
        }

        public DbSet<WalletWise.Models.Expense> Expense { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<WalletWise.Models.Expense>()
                .Property(e => e.Amount)
                .HasPrecision(18, 2);
        }
    }
}