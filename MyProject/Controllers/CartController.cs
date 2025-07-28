using Microsoft.AspNetCore.Mvc;
using MyProject.Models;
using MyProject.Services;

namespace MyProject.Controllers;

public class CartController : Controller
{
    private readonly LaCaffeineContext context;

    public CartController(LaCaffeineContext context)
    {
        this.context = context;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateCartItem([FromBody] UpdateCartItemRequest request)
    {
        try
        {
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();
            var item = cart.FirstOrDefault(x => x.ProductId == request.ProductId);

            if (item != null)
            {
                if (request.Quantity <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    item.Quantity = request.Quantity;
                }

                HttpContext.Session.SetObject("Cart", cart);

                return Json(new
                {
                    success = true,
                    count = cart.Sum(x => x.Quantity),
                    grandTotal = cart.Sum(x => x.Total),
                    itemTotal = item?.Total ?? 0
                });
            }

            return Json(new { success = false, message = "Item not found in cart" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveCartItem([FromBody] RemoveCartItemRequest request)
    {
        try
        {
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();
            var item = cart.FirstOrDefault(x => x.ProductId == request.ProductId);

            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetObject("Cart", cart);

                return Json(new
                {
                    success = true,
                    count = cart.Sum(x => x.Quantity),
                    grandTotal = cart.Sum(x => x.Total)
                });
            }

            return Json(new { success = false, message = "Item not found in cart" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // CartController.cs
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ApplyCoupon([FromBody] ApplyCouponRequest request)
    {
        try
        {
            var cart = HttpContext.Session.GetObject<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();

            // Get the coupon from database
            var coupon = context.Coupons.FirstOrDefault(c =>
                c.CouponCode == request.CouponCode &&
                c.IsActive &&
                c.ExpiryDate >= DateOnly.FromDateTime(DateTime.Today));

            if (coupon == null)
            {
                return Json(new { success = false, message = "Invalid or expired coupon code" });
            }

            // Check if cart contains the item this coupon applies to
            var applicableItem = cart.FirstOrDefault(item =>
                item.ProductName == coupon.ItemName ||
                item.ProductId.ToString() == coupon.ItemName);

            if (applicableItem == null)
            {
                return Json(new
                {
                    success = false,
                    message = $"This coupon is only valid for {coupon.ItemName} which is not in your cart"
                });
            }

            // Apply FIXED RUPEE discount (no percentage calculation)
            applicableItem.AppliedCouponCode = coupon.CouponCode;
            applicableItem.Discount = coupon.Discount; // Directly use the rupee value

            HttpContext.Session.SetObject("Cart", cart);

            return Json(new
            {
                success = true,
                count = cart.Sum(x => x.Quantity),
                grandTotal = cart.Sum(x => x.DiscountedTotal),
                itemTotal = applicableItem.DiscountedTotal,
                discount = applicableItem.Discount,
                affectedItemId = applicableItem.ProductId,
                message = $"Coupon applied! You saved Rs. {applicableItem.Discount}"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }


}