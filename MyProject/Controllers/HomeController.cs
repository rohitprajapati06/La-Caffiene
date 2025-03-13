using Microsoft.AspNetCore.Mvc;

namespace MyProject.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            return View();
        }

    }
}
