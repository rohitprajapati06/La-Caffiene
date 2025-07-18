using System;
using System.Collections.Generic;

namespace MyProject.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string Image { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int Price { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
