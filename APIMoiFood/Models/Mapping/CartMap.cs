using APIMoiFood.Models.Entities;

namespace APIMoiFood.Models.Mapping
{
    public class CartMap
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public List<CartItemMap> CartItems { get; set; } = new List<CartItemMap>();
    }
}
