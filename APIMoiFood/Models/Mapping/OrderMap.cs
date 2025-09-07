namespace APIMoiFood.Models.Mapping
{
    public class OrderMap
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string DeliveryAddress { get; set; } = null!;
        public string? Note { get; set; }
        public decimal TotalAmount { get; set; }
        public string? OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<OrderItemMap> Items { get; set; } = new List<OrderItemMap>();
        public List<PaymentMap> Payments { get; set; } = new List<PaymentMap>();
    }
}
