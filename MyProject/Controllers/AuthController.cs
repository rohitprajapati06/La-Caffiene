using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyProject.Models;
using MyProject.Services;
using System.Security.Claims;


namespace MyProject.Controllers
{
    public class AuthController : Controller
    {
        private readonly LaCaffeineContext _context;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IOtpService otpService;
        private readonly IEmailServices emailServices;

        public AuthController(
            LaCaffeineContext context,
            ITokenService tokenService,
            IPasswordHasher<User> passwordHasher,
            IOtpService otpService,
            IEmailServices emailServices
            )
        {
            _context = context;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            this.otpService = otpService;
            this.emailServices = emailServices;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserRegistrationDto registrationDto)
        {
            if (!ModelState.IsValid)
            {
                return View(registrationDto);
            }
            
            // Check if user already exists
            if (_context.Users.Any(u => u.EmailId == registrationDto.EmailId))
            {
                ModelState.AddModelError("", "User already exists");
                return View(registrationDto);
            }

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = registrationDto.FirstName+" "+registrationDto.LastName,
                EmailId = registrationDto.EmailId,
                FirstName = registrationDto.FirstName,
                LastName = registrationDto.LastName,
                TimeStamp = DateTime.UtcNow,
                Providers = "Local",
                ProfilePhoto = "https://static.vecteezy.com/system/resources/previews/009/292/244/non_2x/default-avatar-icon-of-social-media-user-vector.jpg",

            };

            // Hash the password
            user.Password = _passwordHasher.HashPassword(user, registrationDto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            int otp = await otpService.GenerateOtpAsync();

            TempData["Otp"] = otp.ToString();

            await emailServices.SendEmailAsync(registrationDto.EmailId, "Email Confirmation", $"Your Six Digit Otp is {otp}");

            TempData["SuccessMessage"] = "Registration successful. Please log in.";
            return RedirectToAction("Login", "Auth");
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserLoginDto loginDto, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(loginDto);
            }

            var user = _context.Users.FirstOrDefault(u => u.EmailId == loginDto.EmailId);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid credentials");
                return View(loginDto);
            }

            // Verify password
            var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(
                user, user.Password, loginDto.Password
            );

            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Invalid credentials");
                return View(loginDto);
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.EmailId),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            // Sign in the user
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Redirect to return URL or home
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public IActionResult Profile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = _context.Users.FirstOrDefault(u => u.UserId.ToString() == userId);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult GoogleLogin(string returnUrl = null)
        {
            var redirectUrl = Url.Action("GoogleResponse", "Auth", new { returnUrl });
            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl,
                Items = { { "returnUrl", returnUrl } }
            };
            return Challenge(properties, "Google");
        }

        [HttpGet]
        public async Task<IActionResult> GoogleResponse(string returnUrl = null)
        {
            var authenticateResult = await HttpContext.AuthenticateAsync("Google");

            if (!authenticateResult.Succeeded)
            {
                return RedirectToAction("Login", new { returnUrl = returnUrl });
            }

            // Get user info from Google claims
            var emailClaim = authenticateResult.Principal.FindFirst(ClaimTypes.Email);
            var nameClaim = authenticateResult.Principal.FindFirst(ClaimTypes.Name);
            var idClaim = authenticateResult.Principal.FindFirst(ClaimTypes.NameIdentifier);
            var pictureClaim = authenticateResult.Principal.FindFirst("picture") ??
                   authenticateResult.Principal.FindFirst("urn:google:picture");

            if (emailClaim == null || nameClaim == null)
            {
                return RedirectToAction("Login");
            }

            // Check if the user already exists in your database
            var user = _context.Users.FirstOrDefault(u => u.EmailId == emailClaim.Value);

            if (user == null)
            {
                // Create new user if they don't exist
                string[] nameParts = nameClaim.Value.Split(' ');
                string firstName = nameParts.Length > 0 ? nameParts[0] : "";
                string lastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";

                user = new User
                {
                    UserId = Guid.NewGuid(),
                    Username = nameClaim.Value,
                    EmailId = emailClaim.Value,
                    FirstName = firstName,
                    LastName = lastName,
                    TimeStamp = DateTime.UtcNow,
                    Providers = "Google",
                    ProfilePhoto = pictureClaim?.Value,
                    Password = _passwordHasher.HashPassword(null, Guid.NewGuid().ToString())
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Update existing user with Google info if needed
                if (user.Providers != "Google" && user.Providers != "Local,Google")
                {
                    user.Providers += ",Google";
                    await _context.SaveChangesAsync();
                }
            }

            // Create claims for cookie authentication
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.Email, user.EmailId),
        new Claim(ClaimTypes.Name, user.Username)
    };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            // Sign in the user with cookie authentication
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Redirect to return URL or home
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}