using System;
using System.Collections.Generic;

namespace APIMoiFood.Models.Entities;

public partial class RefreshToken
{
    public int TokenId { get; set; }

    public int? UserId { get; set; }

    public string RefreshToken1 { get; set; } = null!;

    public DateTime ExpiryDate { get; set; }

    public bool? IsRevoked { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
