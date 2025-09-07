namespace APIMoiFood.Models.DTOs.Order
{
    public class OrderItemRequest
    {
        public int FoodId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? Note { get; set; }
    }
}
