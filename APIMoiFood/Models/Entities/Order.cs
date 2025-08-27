using System;
using System.Collections.Generic;

namespace APIMoiFood.Models.Entities;

public partial class Order
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public string? DeliveryAddress { get; set; }

    public string? Note { get; set; }

    public decimal TotalAmount { get; set; }

    public string? OrderStatus { get; set; }

    public string? PaymentStatus { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual User User { get; set; } = null!;
}
