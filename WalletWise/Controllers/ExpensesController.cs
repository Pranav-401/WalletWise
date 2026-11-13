using Microsoft.AspNetCore.Mvc;
using WalletWise.Data.Service;
using WalletWise.Models;

namespace WalletWise.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly IExpensesService _expensesService;
        public ExpensesController(IExpensesService expensesService)
        {
            _expensesService = expensesService;
        }
        public async Task<IActionResult> Index()
        {
            var expenses = await _expensesService.GetAll();
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
            if (ModelState.IsValid)
            {
                await _expensesService.Add(expense);
                return RedirectToAction("Index");
            }

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> GetChart()
        {
            var data = await _expensesService.GetChartData();

            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var expense = await _expensesService.GetById(id);

            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
           int id, Expense expense)
        {
            if (id != expense.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(expense);
            }

            var updated = await _expensesService.Update(expense);

            if (!updated)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var expense = await _expensesService.GetById(id);

            if (expense == null)
            {
                return NotFound();
            }

            return View(expense);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleted = await _expensesService.Delete(id);

            if (!deleted)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
