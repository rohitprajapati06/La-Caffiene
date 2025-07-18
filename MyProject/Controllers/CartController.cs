using Microsoft.AspNetCore.Mvc;
using MyProject.Models;
using MyProject.Services;

namespace MyProject.Controllers;

public class CartController : Controller
{
    [HttpPost]
    public IActionResult AddToCart([FromBody] CartItemViewModel product)
    {
        try
        {
            if (!ModelState.IsValid || product == null)
            {
                return BadRequest(new { success = false, message = "Invalid product data" });
            }

            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();

            var existingItem = cart.FirstOrDefault(x => x.ProductId == product.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cart.Add(new CartItemViewModel
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    Image = product.Image,
                    Quantity = product.Quantity
                });
            }

            HttpContext.Session.SetObject("Cart", cart);

            return Json(new
            {
                success = true,
                count = cart.Sum(x => x.Quantity),
                message = "Item added to cart"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }


    public IActionResult Index()
    {
        var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();
        return View(cart);
    }
}