using Microsoft.EntityFrameworkCore;
using MyProject.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var provider = builder.Services.BuildServiceProvider();
var config = provider.GetRequiredService<IConfiguration>();
builder.Services.AddDbContext<LaCaffeineContext>(item => item.UseSqlServer(config.GetConnectionString("dbcs")));
var app = builder.Build();

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseRouting();

app.Run();
