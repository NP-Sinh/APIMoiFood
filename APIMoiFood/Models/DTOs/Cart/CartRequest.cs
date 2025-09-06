namespace APIMoiFood.Models.DTOs.Cart
{
    public class CartRequest
    {
        public int UserId { get; set; }
        public List<CartItemRequest> Items { get; set; } = new List<CartItemRequest>();
    }
}
