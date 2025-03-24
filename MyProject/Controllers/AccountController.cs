using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MyProject.Models;
using MyProject.Models.User_Management;
using MyProject.Services;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Controllers
{
    public class AccountController : Controller
    {
        private readonly LaCaffeineContext _context;
        private readonly TokenService _tokenService;
        private readonly IConfiguration _config;

        public AccountController(LaCaffeineContext context, TokenService tokenService, IConfiguration config)
        {
            _context = context;
            _tokenService = tokenService;
            _config = config;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _context.Users.AnyAsync(u => u.EmailId == model.EmailId))
            {
                ModelState.AddModelError("", "Email already registered.");
                return View(model);
            }

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = model.FirstName +" "+model.LastName,
                EmailId = model.EmailId,
                Password = HashPassword(model.Password),
                FirstName = model.FirstName,
                LastName = model.LastName,
                TimeStamp = DateTime.UtcNow,
                Providers = "Local",
                ProfilePhoto = "https://static.vecteezy.com/system/resources/thumbnails/013/360/247/small/default-avatar-photo-icon-social-media-profile-sign-symbol-vector.jpg",
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailId == model.Email);

            if (user == null || user.Password != HashPassword(model.Password))
            {
                ModelState.AddModelError("", "Invalid credentials.");
                return View(model);
            }

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            SetRefreshTokenCookie(refreshToken);

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public IActionResult RefreshToken()
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                return Unauthorized("Refresh token missing.");
            }

            // Normally, we would validate refresh token, but since we're storing it only in cookies, just return new access token.
            var user = _context.Users.FirstOrDefault(); // Dummy user for example
            if (user == null) return Unauthorized("Invalid refresh token.");

            var newAccessToken = _tokenService.GenerateAccessToken(user);

            return Json(new { accessToken = newAccessToken });
        }

        [HttpPost]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("refreshToken");
            return RedirectToAction("Login");
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // Ensure HTTPS in production
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(int.Parse(_config["JwtSettings:RefreshTokenExpiryDays"]))
            });
        }

        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
