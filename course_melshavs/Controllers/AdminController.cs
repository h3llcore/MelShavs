using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using course_melshavs.Models;
using Microsoft.EntityFrameworkCore;
using System;

[Route("Admin")]
public class AdminController : Controller
{
    private bool IsAdmin()
    {
        var isAdmin = HttpContext.Session.GetString("IsAdmin");
        return isAdmin != null && bool.Parse(isAdmin);
    }

    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Головна сторінка адмін-панелі
    [HttpGet("AdminPanel")]
    public IActionResult AdminPanel()
    {
        var products = _context.Products.ToList(); // Отримуємо всі страви
        return View(products);
    }

    [HttpGet("Orders")]
    public IActionResult Orders()
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        var orders = _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Include(o => o.User)
            .ToList();

        ViewBag.StatusOptions = new List<SelectListItem>
    {
        new SelectListItem { Value = "Pending", Text = "Обробляється" },
        new SelectListItem { Value = "InProgess", Text = "Готується" },
        new SelectListItem { Value = "InRoute", Text = "У дорозі" },
        new SelectListItem { Value = "Completed", Text = "Завершено" },
        new SelectListItem { Value = "Cancelled", Text = "Відмінено" }
    };
        return View(orders);
    }

    [HttpPost]
    public IActionResult UpdateOrderStatus(int orderId, string status)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        var order = _context.Orders.Find(orderId);
        if (order != null)
        {
            order.Status = status;
            _context.SaveChanges();
        }

        return RedirectToAction("Orders");
    }


    // Підтвердження видалення страви
    [HttpGet("Delete")]
    public IActionResult Delete(int id)
    {
        var product = _context.Products.FirstOrDefault(b => b.Id == id);
        if (product == null)
        {
            return NotFound("Страва не знайдена.");
        }

        return RedirectToAction("Delete");
    }


    // Обробка видалення (POST)
    [HttpPost("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        var product = _context.Products.FirstOrDefault(b => b.Id == id);
        if (product != null)
        {
            _context.Products.Remove(product);
            _context.SaveChanges();
        }

        return RedirectToAction("AdminPanel");
    }


    // Редагування страви (форма)
    [HttpGet("Edit/{id}")]
    public IActionResult Edit(int id)
    {
        var product = _context.Products.FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            return NotFound("Страва не знайдена.");
        }

        return View(product); // Перенаправляємо на форму редагування
    }

    // Редагування страви (обробка)
    [HttpPost("Edit/{id}")]
    public IActionResult Edit(int id, Product updatedProduct, IFormFile file)
    {
        var product = _context.Products.FirstOrDefault(p => p.Id == id);
        if (product != null)
        {
            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Weight = updatedProduct.Weight;
            product.Price = updatedProduct.Price;

            // Оновлення зображення
            if (file != null && file.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Product", fileName);

                // Зберігаємо файл
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                // Видаляємо старий файл, якщо він існує
                if (!string.IsNullOrEmpty(product.ImagePath))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Product", product.ImagePath);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                product.ImagePath = fileName;
            }

            _context.SaveChanges();
        }

        return RedirectToAction("AdminPanel");
    }


    // Створення страви (форма)
    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View();
    }

    // Створення страви (обробка)
    [HttpPost("Create")]
    public async Task<IActionResult> Create(string Name, string Description, decimal Weight, decimal Price, IFormFile file)
    {
        if (_context.Products.Any(p => p.Name == Name))
        {
            ModelState.AddModelError("", "Страва з такою назвою вже існує.");
            return View();
        }

        if (ModelState.IsValid)
        {
            string imagePath = "default-image.jpg"; // Значення за замовчуванням
            if (file != null && file.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Product", fileName);

                // Зберігаємо файл
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                imagePath = fileName;
            }

            // Створення нової страви
            var product = new Product
            {
                Name = Name,
                Description = Description,
                Weight = Weight,
                Price = Price,
                ImagePath = imagePath
            };

            // Додавання страви до бази даних
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction("AdminPanel");
        }

        return View();
    }
}