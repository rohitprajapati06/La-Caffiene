using System;
using System.Collections.Generic;

namespace MyProject.Models;

public partial class Cart
{
    public Guid UserId { get; set; }

    public Guid OrderItemId { get; set; }

    public int OrderId { get; set; }

    public string ProductName { get; set; } = null!;

    public string Image { get; set; } = null!;

    public int Price { get; set; }

    public int Quantity { get; set; }

    public virtual Product Order { get; set; } = null!;
}
