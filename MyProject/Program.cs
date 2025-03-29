using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyProject.Models;
using MyProject.Services;
using MyProject.Services.Mail;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddTransient<Random>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddTransient<IOtpService, OtpService>();
builder.Services.AddTransient<IEmailServices, EmailServices>();

builder.Services.AddDbContext<LaCaffeineContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("dbcs")));

// Configure Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        options.CallbackPath = "/signin-google"; // Default callback path

        // Ensure profile scope is added (important for retrieving profile picture)
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.SaveTokens = true; // Ensures access tokens are saved

        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub");
        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
        options.ClaimActions.MapJsonKey("urn:google:picture", "picture"); // Maps profile picture

        // Custom event handler for additional claim processing
        options.Events.OnCreatingTicket = context =>
        {
            var picture = context.User.GetProperty("picture").GetString();
            if (!string.IsNullOrEmpty(picture))
            {
                context.Identity.AddClaim(new Claim("urn:google:picture", picture));
            }
            return Task.CompletedTask;
        };
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Add Authentication and Authorization Middleware
app.UseAuthentication();
app.UseAuthorization();

// Configure default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Run the application
app.Run();
