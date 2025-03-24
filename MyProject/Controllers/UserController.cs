using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MyProject.Controllers
{
    [Authorize] // 🔐 This ensures only authenticated users can access this controller
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var firstName = User.FindFirst("FirstName")?.Value;
            var lastName = User.FindFirst("LastName")?.Value;

            ViewData["Message"] = "Welcome to the protected page!";
            ViewData["UserId"] = userId;
            ViewData["Email"] = email;
            ViewData["FullName"] = $"{firstName} {lastName}";

            return View();
        }
    }
}
