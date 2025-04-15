using System;
using System.Collections.Generic;

namespace MyProject.Models;

public partial class Order
{
    public Guid OrderId { get; set; }

    public Guid CustomerId { get; set; }

    public DateTime OrderDate { get; set; }

    public long TotalAmount { get; set; }

    public string OrderStatus { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public string DeliveryAddress { get; set; } = null!;

    public DateTime DileveryTime { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public virtual User Customer { get; set; } = null!;
}
