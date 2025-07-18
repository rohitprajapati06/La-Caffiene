using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.ProjectModel;
using MyProject.Models;
using MyProject.Services;
using System.Security.Claims;

namespace MyProject.Controllers;

public class OrderController : Controller
{
    private readonly LaCaffeineContext _context;

    public OrderController(LaCaffeineContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Checkout()
    {
        var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();
        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder()
    {
        var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart");

        if (cart == null || !cart.Any())
            return RedirectToAction("Index", "Home");

        // Get user ID from claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
        {
            return RedirectToAction("Login", "Auth");
        }

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.Now,
            TotalAmount = cart.Sum(i => i.Price * i.Quantity),
            Status = "Pending"
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        foreach (var item in cart)
        {
            _context.OrderItems.Add(new OrderItem
            {
                OrderId = order.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Price
            });
        }

        await _context.SaveChangesAsync();
        HttpContext.Session.Remove("Cart");

        return RedirectToAction("OrderSuccess");
    }

    public IActionResult OrderSuccess() => View();
}

