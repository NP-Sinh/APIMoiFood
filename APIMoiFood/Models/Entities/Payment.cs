using System;
using System.Collections.Generic;

namespace APIMoiFood.Models.Entities;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int OrderId { get; set; }

    public int MethodId { get; set; }

    public decimal Amount { get; set; }

    public string? TransactionId { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual PaymentMethod Method { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
