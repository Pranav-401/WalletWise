
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WalletWise.Data;
using WalletWise.Data.Service;
using WalletWise.Models;

namespace WalletWise.Tests.Services
{
    public class ExpensesServiceTests : IAsyncLifetime
    {
        private SqliteConnection _connection = null!;
        private FinanceAppContext _context = null!;
        private ExpensesService _service = null!;

        // A fresh database is created before every test.
        public async Task InitializeAsync()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            await _connection.OpenAsync();

            var options = new DbContextOptionsBuilder<FinanceAppContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new FinanceAppContext(options);

            await _context.Database.EnsureCreatedAsync();

            _service = new ExpensesService(_context);
        }

        // Dispose the database after every test.
        public async Task DisposeAsync()
        {
            await _context.DisposeAsync();
            await _connection.DisposeAsync();
        }

        // Helper method for creating test expenses.
        private async Task<Expense> AddExpenseAsync(
            string userId,
            string description,
            decimal amount,
            string category,
            DateTime date)
        {
            var expense = new Expense
            {
                UserId = userId,
                Description = description,
                Amount = amount,
                Category = category,
                Date = date
            };

            _context.Expense.Add(expense);
            await _context.SaveChangesAsync();

            return expense;
        }

        // =========================================
        // ADD TESTS
        // =========================================

        [Fact]
        public async Task Add_ShouldSaveExpenseSuccessfully()
        {
            // Arrange
            var expense = new Expense
            {
                UserId = "user1",
                Description = "Groceries",
                Amount = 500m,
                Category = "Food",
                Date = new DateTime(2026, 1, 10)
            };

            // Act
            await _service.Add(expense);

            // Assert
            var savedExpense = await _context.Expense
                .FirstOrDefaultAsync();

            Assert.NotNull(savedExpense);
            Assert.True(savedExpense.Id > 0);
            Assert.Equal("Groceries", savedExpense.Description);
            Assert.Equal(500m, savedExpense.Amount);
            Assert.Equal("Food", savedExpense.Category);
            Assert.Equal("user1", savedExpense.UserId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Add_ShouldThrowException_WhenUserIdIsMissing(
            string? userId)
        {
            // Arrange
            var expense = new Expense
            {
                UserId = userId!,
                Description = "Groceries",
                Amount = 500m,
                Category = "Food"
            };

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.Add(expense));

            // Assert
            Assert.Equal("User ID is required.", exception.Message);
            Assert.Empty(await _context.Expense.ToListAsync());
        }

        // =========================================
        // GET ALL TESTS
        // =========================================

        [Fact]
        public async Task GetAll_ShouldReturnOnlyCurrentUsersExpenses()
        {
            // Arrange
            var date1 = new DateTime(2026, 1, 10);
            var date2 = new DateTime(2026, 1, 15);

            await AddExpenseAsync(
                "user1", "Groceries", 500m, "Food", date1);

            await AddExpenseAsync(
                "user2", "Shopping", 1000m, "Shopping", date2);

            await AddExpenseAsync(
                "user1", "Transport", 200m, "Travel", date2);

            // Act
            var result = (await _service.GetAll("user1")).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, e => Assert.Equal("user1", e.UserId));
        }

        [Fact]
        public async Task GetAll_ShouldReturnExpensesInDescendingDateOrder()
        {
            // Arrange
            await AddExpenseAsync(
                "user1", "Old expense", 100m, "Food",
                new DateTime(2026, 1, 1));

            await AddExpenseAsync(
                "user1", "New expense", 200m, "Food",
                new DateTime(2026, 1, 20));

            // Act
            var result = (await _service.GetAll("user1")).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("New expense", result[0].Description);
            Assert.Equal("Old expense", result[1].Description);
        }

        [Fact]
        public async Task GetAll_ShouldReturnEmptyCollection_WhenUserHasNoExpenses()
        {
            // Act
            var result = await _service.GetAll("user1");

            // Assert
            Assert.Empty(result);
        }

        // =========================================
        // GET CHART DATA TESTS
        // =========================================

        [Fact]
        public async Task GetChartData_ShouldCalculateCategoryTotalsForCurrentUser()
        {
            // Arrange
            await AddExpenseAsync(
                "user1", "Groceries", 500m, "Food", DateTime.Today);

            await AddExpenseAsync(
                "user1", "Restaurant", 300m, "Food", DateTime.Today);

            await AddExpenseAsync(
                "user1", "Bus", 200m, "Travel", DateTime.Today);

            // Another user's expense must not be included.
            await AddExpenseAsync(
                "user2", "Shopping", 1000m, "Food", DateTime.Today);

            // Act
            var result = await _service.GetChartData("user1");

            // Assert
            Assert.Equal(2, result.Count);

            var food = Assert.Single(
                result.Where(x => x.Category == "Food"));

            Assert.Equal(800m, food.TotalAmount);

            var travel = Assert.Single(
                result.Where(x => x.Category == "Travel"));

            Assert.Equal(200m, travel.TotalAmount);
        }

        [Fact]
        public async Task GetChartData_ShouldReturnEmptyCollection_WhenUserHasNoExpenses()
        {
            // Act
            var result = await _service.GetChartData("user1");

            // Assert
            Assert.Empty(result);
        }

        // =========================================
        // GET BY ID TESTS
        // =========================================

        [Fact]
        public async Task GetById_ShouldReturnExpense_WhenExpenseBelongsToUser()
        {
            // Arrange
            var expense = await AddExpenseAsync(
                "user1", "Groceries", 500m, "Food", DateTime.Today);

            // Act
            var result = await _service.GetById(expense.Id, "user1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expense.Id, result.Id);
            Assert.Equal("Groceries", result.Description);
        }

        [Fact]
        public async Task GetById_ShouldReturnNull_WhenExpenseDoesNotExist()
        {
            // Act
            var result = await _service.GetById(999, "user1");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetById_ShouldReturnNull_WhenExpenseBelongsToAnotherUser()
        {
            // Arrange
            var expense = await AddExpenseAsync(
                "user2", "Shopping", 1000m, "Shopping", DateTime.Today);

            // Act
            var result = await _service.GetById(expense.Id, "user1");

            // Assert
            Assert.Null(result);
        }

        // =========================================
        // UPDATE TESTS
        // =========================================

        [Fact]
        public async Task Update_ShouldUpdateExpense_WhenExpenseBelongsToUser()
        {
            // Arrange
            var originalDate = new DateTime(2026, 1, 1);

            var expense = await AddExpenseAsync(
                "user1", "Old description", 500m, "Food", originalDate);

            var updatedExpense = new Expense
            {
                Id = expense.Id,
                UserId = "user1",
                Description = "Updated description",
                Amount = 750m,
                Category = "Shopping",
                Date = new DateTime(2026, 1, 15)
            };

            // Act
            var result = await _service.Update(updatedExpense, "user1");

            // Assert
            Assert.True(result);

            var savedExpense = await _context.Expense
                .FirstAsync(e => e.Id == expense.Id);

            Assert.Equal("Updated description", savedExpense.Description);
            Assert.Equal(750m, savedExpense.Amount);
            Assert.Equal("Shopping", savedExpense.Category);
            Assert.Equal(new DateTime(2026, 1, 15), savedExpense.Date);
        }

        [Fact]
        public async Task Update_ShouldReturnFalse_WhenExpenseDoesNotExist()
        {
            // Arrange
            var expense = new Expense
            {
                Id = 999,
                Description = "Updated",
                Amount = 500m,
                Category = "Food",
                Date = DateTime.Today
            };

            // Act
            var result = await _service.Update(expense, "user1");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Update_ShouldReturnFalse_WhenExpenseBelongsToAnotherUser()
        {
            // Arrange
            var expense = await AddExpenseAsync(
                "user2", "Shopping", 1000m, "Shopping", DateTime.Today);

            var updatedExpense = new Expense
            {
                Id = expense.Id,
                Description = "Hacked description",
                Amount = 1m,
                Category = "Food",
                Date = DateTime.Today
            };

            // Act
            var result = await _service.Update(updatedExpense, "user1");

            // Assert
            Assert.False(result);

            var savedExpense = await _context.Expense
                .FirstAsync(e => e.Id == expense.Id);

            Assert.Equal("Shopping", savedExpense.Description);
            Assert.Equal(1000m, savedExpense.Amount);
            Assert.Equal("user2", savedExpense.UserId);
        }

        // =========================================
        // DELETE TESTS
        // =========================================

        [Fact]
        public async Task Delete_ShouldDeleteExpense_WhenExpenseBelongsToUser()
        {
            // Arrange
            var expense = await AddExpenseAsync(
                "user1", "Groceries", 500m, "Food", DateTime.Today);

            // Act
            var result = await _service.Delete(expense.Id, "user1");

            // Assert
            Assert.True(result);

            var deletedExpense = await _context.Expense
                .FindAsync(expense.Id);

            Assert.Null(deletedExpense);
        }

        [Fact]
        public async Task Delete_ShouldReturnFalse_WhenExpenseDoesNotExist()
        {
            // Act
            var result = await _service.Delete(999, "user1");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Delete_ShouldReturnFalse_WhenExpenseBelongsToAnotherUser()
        {
            // Arrange
            var expense = await AddExpenseAsync(
                "user2", "Shopping", 1000m, "Shopping", DateTime.Today);

            // Act
            var result = await _service.Delete(expense.Id, "user1");

            // Assert
            Assert.False(result);

            var savedExpense = await _context.Expense
                .FindAsync(expense.Id);

            Assert.NotNull(savedExpense);
            Assert.Equal("user2", savedExpense.UserId);
        }
    }
}
