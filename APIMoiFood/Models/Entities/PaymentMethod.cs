using System;
using System.Collections.Generic;

namespace APIMoiFood.Models.Entities;

public partial class PaymentMethod
{
    public int MethodId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
