using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course_melshavs.Models;
using System.Linq;

namespace course_melshavs.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                ViewBag.Message = "Ваш кошик порожній!";
                return View(new List<CartItem>());
            }

            return View(cart.CartItems);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            // Отримуємо ID користувача з сесії
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account"); // Якщо користувач не увійшов у систему, перенаправляємо на сторінку логіну
            }

            // Перевіряємо, чи існує продукт з вказаним ID
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                ModelState.AddModelError("", "Страву не знайдено.");
                return RedirectToAction("Index", "Home");
            }

            // Отримуємо кошик користувача або створюємо новий, якщо кошика ще немає
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId.Value, CartItems = new List<CartItem>() };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync(); // Зберігаємо новий кошик у базі даних
            }

            // Перевіряємо, чи продукт вже є у кошику
            var cartItem = cart.CartItems.FirstOrDefault(i => i.ProductId == productId);
            if (cartItem == null)
            {
                // Якщо продукту немає в кошику, додаємо його
                cartItem = new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity,
                    Price = product.Price // Зберігаємо поточну ціну продукту
                };
                cart.CartItems.Add(cartItem);
            }
            else
            {
                // Якщо продукт вже в кошику, збільшуємо кількість
                cartItem.Quantity += quantity;
            }

            // Оновлюємо кількість товарів у кошику в сесії
            HttpContext.Session.SetInt32("CartItemCount", cart.CartItems.Sum(ci => ci.Quantity));

            await _context.SaveChangesAsync(); // Зберігаємо зміни у базі даних
            return RedirectToAction("Index");
        }


        [HttpPost]
        public IActionResult RemoveFromCart(int cartItemId)
        {
            // Знаходимо товар у кошику за ID
            var cartItem = _context.CartItems.Find(cartItemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem); // Видаляємо товар з кошика
                _context.SaveChanges();

                // Оновлюємо кількість товарів у кошику в сесії
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId != null)
                {
                    var cart = _context.Carts
                        .Include(c => c.CartItems)
                        .FirstOrDefault(c => c.UserId == userId);

                    // Оновлюємо кількість товарів у сесії або встановлюємо 0, якщо кошик порожній
                    HttpContext.Session.SetInt32("CartItemCount", cart?.CartItems.Sum(ci => ci.Quantity) ?? 0);
                }
            }

            return RedirectToAction("Index");
        }


        [HttpGet]
        public IActionResult Checkout()
        {
            // Отримуємо ID користувача з сесії
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account"); // Якщо користувач не увійшов, перенаправляємо

            // Завантажуємо кошик користувача
            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                return RedirectToAction("Index"); // Якщо кошик порожній, повертаємо на головну
            }

            return View(new CheckoutViewModel()); // Відображаємо форму для введення інформації
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Checkout", model); // Якщо модель невалідна, повертаємо форму з помилками
            }

            // Отримуємо ID користувача з сесії
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account"); // Якщо користувач не увійшов, перенаправляємо
            }

            // Завантажуємо кошик користувача
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                return RedirectToAction("Index", "Cart"); // Якщо кошик порожній, повертаємо
            }

            // Створюємо нове замовлення
            var order = new Order
            {
                UserId = userId.Value,
                OrderDate = DateTime.Now,
                Status = "Pending", // Статус за замовчуванням
                Items = cart.CartItems.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                }).ToList(),
                Address = model.Address,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                AdditionalNotes = model.AdditionalNotes ?? string.Empty
            };

            _context.Orders.Add(order);
            _context.Carts.Remove(cart); // Видаляємо кошик після оформлення замовлення
            await _context.SaveChangesAsync();

            return RedirectToAction("OrderConfirmation", new { orderId = order.Id }); // Перенаправляємо на сторінку підтвердження
        }

        [HttpGet]
        public IActionResult OrderConfirmation(int orderId)
        {
            // Завантажуємо замовлення за ID
            var order = _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound(); // Якщо замовлення не знайдено, повертаємо 404
            }

            return View(order); // Відображаємо замовлення
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int cartItemId, string action)
        {
            // Знаходимо товар у кошику
            var cartItem = _context.CartItems.Find(cartItemId);
            if (cartItem != null)
            {
                if (action == "increase")
                {
                    cartItem.Quantity++; // Збільшуємо кількість
                }
                else if (action == "decrease" && cartItem.Quantity > 1)
                {
                    cartItem.Quantity--; // Зменшуємо кількість, якщо більше 1
                }

                _context.SaveChanges();

                // Оновлюємо кількість товарів у кошику в сесії
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId != null)
                {
                    var cart = _context.Carts
                        .Include(c => c.CartItems)
                        .FirstOrDefault(c => c.UserId == userId);

                    HttpContext.Session.SetInt32("CartItemCount", cart?.CartItems.Sum(ci => ci.Quantity) ?? 0);
                }
            }

            return RedirectToAction("Index");
        }
    }
}
