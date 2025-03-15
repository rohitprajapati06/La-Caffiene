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
            var coupons = await context.Coupons.ToListAsync();
            var products = await context.Products.ToListAsync(); // Fetch menu items

            var viewModel = new HomeViewModel
            {
                Coupons = coupons,
                Products = products,
            };

            return View(viewModel);
        }

    }
}
