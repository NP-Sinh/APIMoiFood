using APIMoiFood.Models.Entities;

namespace APIMoiFood.Models.Mapping
{
    public class FoodMap
    {
        public int FoodId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        public int? CategoryId { get; set; }

        public bool? IsAvailable { get; set; }
        public bool? IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public virtual List<CartItemMap> CartItems { get; set; } = new List<CartItemMap>();

        public virtual List<OrderItemMap> OrderItems { get; set; } = new List<OrderItemMap>();

        public virtual List<ReviewMap> Reviews { get; set; } = new List<ReviewMap>();
    }
}
