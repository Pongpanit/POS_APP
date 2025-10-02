using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POS_APP.Models;
using POS_APP.Extensions;

namespace POS_APP.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        // ใช้ session เก็บตะกร้า
        private const string CartSessionKey = "Cart";

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ แสดงหน้า Order (เมนู + ตะกร้า)
        public async Task<IActionResult> Index(int? activeCategoryId)
        {
            // ถ้ามีการส่ง activeCategoryId → เก็บลง Session
            if (activeCategoryId != null)
                HttpContext.Session.SetInt32("ActiveCategoryId", activeCategoryId.Value);

            var categories = await _context.Categories
                                           .Include(c => c.Products)
                                           .ToListAsync();

            // ดึงข้อมูลตะกร้าจาก Session
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey)
                       ?? new List<CartItem>();

            ViewBag.Cart = cart;
            ViewBag.Total = cart.Sum(c => c.Total);

            // อ่าน activeCategoryId จาก Session (ถ้าไม่มีใช้ Category แรก)
            ViewBag.ActiveCategoryId = HttpContext.Session.GetInt32("ActiveCategoryId")
                                       ?? categories.First().CategoryId;

            return View(categories);
        }

        // ✅ เพิ่มเมนูเข้าตะกร้า
        [HttpPost]
        public IActionResult AddToCart(int productId, int categoryId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey)
                       ?? new List<CartItem>();

            var product = _context.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product != null)
            {
                var item = cart.FirstOrDefault(c => c.ProductId == productId);
                if (item != null)
                {
                    item.Quantity++;
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        ProductId = product.ProductId,
                        Name = product.Name,
                        Price = product.Price,
                        Quantity = 1
                    });
                }
            }

            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);

            // เก็บ activeCategoryId ล่าสุด
            HttpContext.Session.SetInt32("ActiveCategoryId", categoryId);

            return RedirectToAction("Index");
        }

        // ✅ เพิ่มจำนวน
        [HttpPost]
        public IActionResult IncreaseQuantity(int productId, int categoryId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                item.Quantity++;
            }
            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);

            // 👉 redirect ไปที่หมวดหมู่เดิม
            return RedirectToAction("Index", new { activeCategoryId = categoryId });
        }

        // ✅ ลดจำนวน
        [HttpPost]
        public IActionResult DecreaseQuantity(int productId, int categoryId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null && item.Quantity > 1)
            {
                item.Quantity--;
            }
            else if (item != null && item.Quantity == 1)
            {
                cart.Remove(item);
            }
            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);

            // 👉 redirect ไปที่หมวดหมู่เดิม
            return RedirectToAction("Index", new { activeCategoryId = categoryId });
        }

        // ✅ ลบจากตะกร้า
        [HttpPost]
        public IActionResult RemoveFromCart(int productId, int categoryId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
            }

            // 👉 redirect ไปที่หมวดหมู่เดิม
            return RedirectToAction("Index", new { activeCategoryId = categoryId });
        }

        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            if (!cart.Any())
            {
                TempData["CheckoutError"] = "❌ Please add at least one item to your cart";
                return RedirectToAction("Index");
            }

            var order = new Order
            {
                OrderDate = DateTime.Now,
                Total = cart.Sum(c => c.Total),
                Items = cart.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity,
                    Price = c.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            //Clear cart
            HttpContext.Session.Remove(CartSessionKey);

            TempData["CheckoutSuccess"] = "Order has been successfully saved!";
            return RedirectToAction("Index");
        }
    }
}
