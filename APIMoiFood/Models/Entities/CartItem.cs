using System;
using System.Collections.Generic;

namespace APIMoiFood.Models.Entities;

public partial class CartItem
{
    public int CartItemId { get; set; }

    public int? CartId { get; set; }

    public int? FoodId { get; set; }

    public int Quantity { get; set; }

    public virtual Cart? Cart { get; set; }

    public virtual Food? Food { get; set; }
}
