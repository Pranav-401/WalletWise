using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WalletWise.Data.Service;
using WalletWise.Models;

namespace WalletWise.Controllers
{
    [Authorize]
    public class ExpensesController : Controller
    {
        private readonly IExpensesService _expensesService;

        public ExpensesController(IExpensesService expensesService)
        {
            _expensesService = expensesService;
        }

        private string? CurrentUserId =>
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        public async Task<IActionResult> Index()
        {
            if (CurrentUserId == null)
                return Unauthorized();

            var expenses = await _expensesService.GetAll(CurrentUserId);
            return View(expenses);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Expense expense)
        {
            var userId = CurrentUserId;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            expense.UserId = userId;

            ModelState.Remove(nameof(Expense.UserId));

            if (!ModelState.IsValid)
                return View(expense);

            await _expensesService.Add(expense);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetChart()
        {
            if (CurrentUserId == null)
                return Unauthorized();

            var data = await _expensesService.GetChartData(CurrentUserId);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (string.IsNullOrEmpty(CurrentUserId))
                return Unauthorized();

            var expense = await _expensesService.GetById(id, CurrentUserId);

            if (expense == null)
                return NotFound();

            return View(expense);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Expense expense)
        {
            if (string.IsNullOrEmpty(CurrentUserId))
                return Unauthorized();

            if (id != expense.Id)
                return BadRequest();

            // Set ownership from the authenticated user
            expense.UserId = CurrentUserId;

            // UserId is assigned by the server, not the form
            ModelState.Remove(nameof(Expense.UserId));

            if (!ModelState.IsValid)
                return View(expense);

            var updated = await _expensesService.Update(
                expense, CurrentUserId);

            if (!updated)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (string.IsNullOrEmpty(CurrentUserId))
                return Unauthorized();

            var expense = await _expensesService.GetById(id, CurrentUserId);

            if (expense == null)
                return NotFound();

            return View(expense);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (string.IsNullOrEmpty(CurrentUserId))
                return Unauthorized();

            var deleted = await _expensesService.Delete(
                id, CurrentUserId);

            if (!deleted)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }
    }
}