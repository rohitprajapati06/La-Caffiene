using System;
using System.Collections.Generic;

namespace MyProject.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public Guid UserId { get; set; }

    public DateTime OrderDate { get; set; }

    public int TotalAmount { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual User User { get; set; } = null!;
}
