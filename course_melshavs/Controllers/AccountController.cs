using course_melshavs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace course_melshavs.Controllers
{
    public class AccountController : Controller
    {

        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string Username, string Password, string ConfirmPassword, string? PhoneNumber)
        {
            // Перевірка збігу паролів
            if (Password != ConfirmPassword) // Перевірка відповідності пароля
            {
                ModelState.AddModelError("ConfirmPassword", "Паролі не співпадають.");
            }

            // Перевірка унікальності імені користувача
            if (_context.Users.Any(u => u.Username == Username))
            {
                ModelState.AddModelError(nameof(Username), "Користувач із таким ім'ям уже існує.");
            }

            if (ModelState.IsValid)
            {
                // Створення нового користувача
                var user = new User
                {
                    Username = Username,
                    PhoneNumber = PhoneNumber,
                    Password = Password,
                    ConfirmPassword = Password, // Забезпечити узгодженість даних
                    IsAdmin = false
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("Login");
            }

            return View();
        }

        // Сторінка входу (GET)
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Обработка входа (POST)
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.SingleOrDefault(u => u.Username == username && u.Password == password);
            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());

                return RedirectToAction("Index", "Home");
            }

            // Виведення помилки 
            ViewBag.ErrorMessage = "Неправильне ім'я користувача або пароль.";
            return View();
        }

        // GET: Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Очистка сесії
            return RedirectToAction("Login");
        }

        // Страница профілю (GET)
        [HttpGet]
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = _context.Users.SingleOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user); 
        }

        [HttpGet]
        public IActionResult OrderHistory()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var orders = _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .ToList();

            return View(orders);
        }

        [HttpGet]
        public IActionResult ProfileEdit()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = _context.Users.Find(userId);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditProfileViewModel
            {
                Username = user.Username,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult ProfileEdit(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.Find(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Перевірка поточного пароля
            if (user.Password != model.CurrentPassword)
            {
                ModelState.AddModelError("CurrentPassword", "Неправильний пароль.");
                return View(model);
            }

            // Перевірка унікальності імені користувача
            var existingUser = _context.Users.FirstOrDefault(u => u.Username == model.Username && u.Id != userId);
            if (existingUser != null)
            {
                ModelState.AddModelError("Username", "Це ім'я користувача вже зайнято.");
                return View(model);
            }

            // Оновлення даних
            user.Username = model.Username;
            user.PhoneNumber = model.PhoneNumber;

            _context.SaveChanges();

            return RedirectToAction("Profile");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = _context.Users.Find(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (user.Password != oldPassword)
            {
                ModelState.AddModelError("", "Старий пароль неправильний.");
                return View();
            }

            if (newPassword != confirmPassword) // NEW: Password matching check
            {
                ModelState.AddModelError("", "Паролі не співпадають.");
                return View();
            }

            user.Password = newPassword;
            user.ConfirmPassword = confirmPassword; // NEW: Ensure consistency in data
            _context.SaveChanges();

            return RedirectToAction("Profile");
        }

    }
}
