using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS_APP.Models;

namespace POS_APP.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var totalSales = orders.Sum(o => o.Total);
            var totalOrders = orders.Count;
            var totalItems = orders.Sum(o => o.Items.Sum(i => i.Quantity));

            ViewBag.TotalSales = totalSales;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalItems = totalItems;

            return View(orders);
        }

        [HttpPost]
        public IActionResult ClearAll()
        {
            var orders = _context.Orders.ToList();
            if (orders.Any())
            {
                _context.Orders.RemoveRange(orders);
                _context.SaveChanges();

                // Reset OrderId (SQLite)
                _context.Database.ExecuteSqlRaw("DELETE FROM sqlite_sequence WHERE name = 'Orders';");

                TempData["Message"] = "All bills cleared and OrderId reset!";
            }
            else
            {
                TempData["Message"] = "No bills to clear.";
            }

            return RedirectToAction("Index");
        }
    }
}
