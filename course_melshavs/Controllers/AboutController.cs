using Microsoft.AspNetCore.Mvc;

namespace course_melshavs.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
