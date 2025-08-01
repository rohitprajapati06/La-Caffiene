using Microsoft.AspNetCore.Mvc;
using Razorpay.Api;
using MyProject.Models;
using System.Security.Claims;
using MyProject.Services;

namespace MyProject.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly LaCaffeineContext _context;

        public PaymentController(IConfiguration configuration, LaCaffeineContext context)
        {
            _configuration = configuration;
            _context = context;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateRazorpayOrder()
        {
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new();
            if (!cart.Any()) return Json(new { success = false, message = "Cart is empty" });

            int totalAmount = cart.Sum(x => x.DiscountedTotal > 0 ? x.DiscountedTotal : x.Total) * 100;

            var client = new RazorpayClient(_configuration["Razorpay:Key"], _configuration["Razorpay:Secret"]);

            Dictionary<string, object> options = new()
            {
                { "amount", totalAmount },
                { "currency", "INR" },
                { "receipt", Guid.NewGuid().ToString() },
                { "payment_capture", 1 }
            };

            Razorpay.Api.Order order = client.Order.Create(options);
            return Json(new
            {
                success = true,
                orderId = order["id"].ToString(),
                key = _configuration["Razorpay:Key"],
                amount = totalAmount
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveOrder([FromBody] RazorpayPaymentDto paymentData)
        {
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new();
            if (!cart.Any()) return Json(new { success = false, message = "Cart is empty" });

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdString, out Guid userId))
                return Json(new { success = false, message = "Invalid user ID" });

            var order = new Models.Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                Status = "Paid",
                TotalAmount = cart.Sum(x => x.DiscountedTotal > 0 ? x.DiscountedTotal : x.Total),
                OrderItems = cart.Select(x => new OrderItem
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    Price = x.DiscountedTotal > 0 ? x.DiscountedTotal : x.Total
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.SaveChanges();
            HttpContext.Session.Remove("Cart");

            // Store a flag in TempData to indicate successful payment
            TempData["PaymentSuccess"] = true;

            return Json(new { success = true, message = "Payment successful, order saved!" });
        }

        public IActionResult Success()
        {
            // Check if the payment was successful
            if (TempData["PaymentSuccess"] == null || !(bool)TempData["PaymentSuccess"])
            {
                return RedirectToAction("Index", "Home"); // Redirect to home or another appropriate page
            }

            // Clear the TempData to prevent reuse
            TempData.Remove("PaymentSuccess");

            return View();
        }

        
    }
}
