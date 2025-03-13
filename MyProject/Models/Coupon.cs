using System;
using System.Collections.Generic;

namespace MyProject.Models;

public partial class Coupon
{
    public int Id { get; set; }

    public string ItemName { get; set; } = null!;

    public string Image { get; set; } = null!;

    public string CouponCode { get; set; } = null!;

    public int Discount { get; set; }

    public DateOnly ExpiryDate { get; set; }

    public bool IsActive { get; set; }
}
