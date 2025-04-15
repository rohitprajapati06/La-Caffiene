using System;
using System.Collections.Generic;

namespace MyProject.Models;

public partial class OrderItem
{
    public int OrderItemId { get; set; }

    public Guid OrderId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public long PriceAtOrderTime { get; set; }

    public virtual Product Product { get; set; } = null!;
}
