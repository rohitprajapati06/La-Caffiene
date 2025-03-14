using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Models;

namespace MyProject.Controllers
{
    public class MenuController : Controller
    {
        private readonly LaCaffeineContext context;

        public MenuController(LaCaffeineContext context)
        {
            this.context = context;
        }
        public async Task<IActionResult> Menu()
        {
            var data = await context.Products.ToListAsync();
            return View(data);
        }

    }
}
