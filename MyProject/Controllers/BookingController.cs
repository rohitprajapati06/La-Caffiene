using Microsoft.AspNetCore.Mvc;

namespace MyProject.Controllers
{
    public class BookingController : Controller
    {
        public IActionResult Index()
        {

            return View();
        }
    }
}
