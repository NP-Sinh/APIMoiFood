namespace APIMoiFood.Models.DTOs.Payment
{
    public class PaymentRequest
    {
        public int OrderId { get; set; }
        public long Amount { get; set; }
    }
}
