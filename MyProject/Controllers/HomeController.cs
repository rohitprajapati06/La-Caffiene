using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Models;

namespace MyProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly LaCaffeineContext context;

        public HomeController(LaCaffeineContext context)
        {
            this.context = context;
        }
        public async Task<IActionResult> Index()
        {
            var coupons = await context.Products.ToListAsync(); // ✅ Await the async method
            return View(coupons);
        }

    }
}
