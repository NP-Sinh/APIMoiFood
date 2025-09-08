namespace APIMoiFood.Models.Mapping
{
    public class PaymentMap
    {
        public int PaymentId { get; set; }

        public int OrderId { get; set; }

        public int MethodId { get; set; }

        public decimal Amount { get; set; }

        public string? TransactionId { get; set; }

        public string PaymentStatus { get; set; } = null!;

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
