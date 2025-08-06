using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Models;
using MyProject.Services;
using System.Security.Claims;


namespace MyProject.Controllers;

public class AuthController(
    LaCaffeineContext context,
    ITokenService tokenService,
    IPasswordHasher<User> passwordHasher,
    IOtpService otpService,
    IEmailServices emailServices
        ) : Controller
{
    private readonly LaCaffeineContext _context = context;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;
    private readonly IOtpService otpService = otpService;
    private readonly IEmailServices emailServices = emailServices;

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

        int otp = await otpService.GenerateOtpAsync();

        // Store OTP temporarily (ideally in DB or cache)
        TempData["Otp"] = otp.ToString();
        TempData["UserData"] = Newtonsoft.Json.JsonConvert.SerializeObject(registrationDto);

        await emailServices.SendEmailAsync(registrationDto.EmailId, "Email Confirmation", $"Your Six Digit OTP is {otp}");

        return RedirectToAction("VerifyOtp", "Auth");
    }

    [HttpGet]
    public IActionResult VerifyOtp()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> VerifyOtp(OtpVerificationDto otpDto)
    {
        if (!ModelState.IsValid)
        {
            return View(otpDto); // Return view with validation messages
        }

        if (TempData["Otp"] == null || TempData["UserData"] == null)
        {
            ModelState.AddModelError("", "OTP expired. Please register again.");
            return RedirectToAction("Register");
        }

        string storedOtp = TempData["Otp"].ToString();

        if (otpDto.OtpInput != storedOtp)
        {
            ModelState.AddModelError("", "Invalid OTP. Please try again.");
            return View(otpDto);
        }

        var registrationDto = Newtonsoft.Json.JsonConvert.DeserializeObject<UserRegistrationDto>(TempData["UserData"].ToString());

        Guid userId = Guid.NewGuid();
        string username = registrationDto.FirstName + " " + registrationDto.LastName;
        string passwordhash = _passwordHasher.HashPassword(null, registrationDto.Password);
        string profilephoto = "https://static.vecteezy.com/system/resources/previews/009/292/244/non_2x/default-avatar-icon-of-social-media-user-vector.jpg";
        DateTime timestamp = DateTime.Now;

        await _context.Database.ExecuteSqlRawAsync("EXEC RegisterUser @p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8",
            userId , username , registrationDto.EmailId , passwordhash , timestamp , profilephoto , "Local",registrationDto.FirstName , registrationDto.LastName);

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
    public async Task<IActionResult> Profile()
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

        // Get user's bookings
        var bookings = await _context.Bookings
            .Where(b => b.UserId == Guid.Parse(userId))
            .ToListAsync();

        // Get user's orders
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == Guid.Parse(userId))
            .ToListAsync();

        ViewBag.Bookings = bookings;
        ViewBag.Orders = orders;

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

    [HttpGet]
    public IActionResult FacebookLogin(string returnUrl = null)
    {
        var redirectUrl = Url.Action("FacebookResponse", "Auth", new { returnUrl });
        var properties = new AuthenticationProperties
        {
            RedirectUri = redirectUrl,
            Items = { { "returnUrl", returnUrl } }
        };
        return Challenge(properties, "Facebook");
    }

    [HttpGet]
    public async Task<IActionResult> FacebookResponse(string returnUrl = null)
    {
        var authenticateResult = await HttpContext.AuthenticateAsync("Facebook");

        if (!authenticateResult.Succeeded)
        {
            return RedirectToAction("Login", new { returnUrl = returnUrl });
        }

        // Get user info from Facebook claims
        var emailClaim = authenticateResult.Principal.FindFirst(ClaimTypes.Email);
        var nameClaim = authenticateResult.Principal.FindFirst(ClaimTypes.Name);
        var idClaim = authenticateResult.Principal.FindFirst(ClaimTypes.NameIdentifier);

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

            // Get Facebook profile picture URL
            string profilePictureUrl = $"https://graph.facebook.com/{idClaim.Value}/picture?type=large";

            user = new User
            {
                UserId = Guid.NewGuid(),
                Username = nameClaim.Value,
                EmailId = emailClaim.Value,
                FirstName = firstName,
                LastName = lastName,
                TimeStamp = DateTime.UtcNow,
                Providers = "Facebook",
                ProfilePhoto = profilePictureUrl,
                Password = _passwordHasher.HashPassword(null, Guid.NewGuid().ToString())
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        else
        {
            // Update existing user with Facebook info if needed
            if (!user.Providers.Contains("Facebook"))
            {
                user.Providers += user.Providers.Length > 0 ? ",Facebook" : "Facebook";

                // Update profile photo if user has default one
                if (user.ProfilePhoto == "https://static.vecteezy.com/system/resources/previews/009/292/244/non_2x/default-avatar-icon-of-social-media-user-vector.jpg" && idClaim != null)
                {
                    user.ProfilePhoto = $"https://graph.facebook.com/{idClaim.Value}/picture?type=large";
                }

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