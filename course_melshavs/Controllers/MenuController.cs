using Microsoft.AspNetCore.Mvc;
using System.Linq;
using course_melshavs.Models;

namespace course_melshavs.Controllers
{
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MenuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Відображення каталогу продуктів
        public IActionResult Index()
        {
            // Отримуємо список продуктів із бази даних
            var products = _context.Products.ToList();
            return View(products); // Передаємо список продуктів у подання
        }
    }
}
