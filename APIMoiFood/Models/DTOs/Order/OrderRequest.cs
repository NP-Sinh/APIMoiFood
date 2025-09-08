namespace APIMoiFood.Models.DTOs.Order
{
    public class OrderRequest
    {
        public int UserId { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? Note { get; set; }
        public List<OrderItemRequest> OrderItems { get; set; } = new List<OrderItemRequest>();
        public int PaymentMethodId { get; set; }
        public bool FromCart { get; set; }
    }
}
