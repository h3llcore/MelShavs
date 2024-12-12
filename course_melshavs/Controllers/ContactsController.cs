using Microsoft.AspNetCore.Mvc;

namespace course_melshavs.Controllers
{
    public class ContactsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
