namespace APIMoiFood.Models.Mapping
{
    public class PaymentMap
    {
        public string Method { get; set; } = null!; 
        public decimal Amount { get; set; }
        public string Status { get; set; } = null!; 
        public string? TransactionId { get; set; }
    }
}
