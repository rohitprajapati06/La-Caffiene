﻿namespace MyProject.Models;

public class CartItemViewModel
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;

    private int _quantity = 1;
    public int Quantity
    {
        get => _quantity;
        set => _quantity = value < 1 ? 1 : value;
    }

    public int Price { get; set; }
    public int Total => Price * Quantity;

    public string AppliedCouponCode { get; set; } = string.Empty;
    public int Discount { get; set; } = 0;
    public int DiscountedTotal => Total - Discount;
}
