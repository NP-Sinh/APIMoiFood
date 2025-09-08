using APIMoiFood.Models.Entities;

namespace APIMoiFood.Models.Mapping
{
    public class OrderMap
    {
        public int OrderId { get; set; }

        public int UserId { get; set; }

        public string? DeliveryAddress { get; set; }

        public string? Note { get; set; }

        public decimal TotalAmount { get; set; }

        public string? OrderStatus { get; set; }

        public string? PaymentStatus { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public List<OrderItemMap> OrderItems { get; set; } = new List<OrderItemMap>();
        public List<PaymentMap> Payments { get; set; } = new List<PaymentMap>();
    }
}
