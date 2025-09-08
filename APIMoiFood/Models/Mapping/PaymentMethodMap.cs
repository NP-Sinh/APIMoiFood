using APIMoiFood.Models.Entities;

namespace APIMoiFood.Models.Mapping
{
    public class PaymentMethodMap
    {
        public int MethodId { get; set; }

        public string Name { get; set; } = null!;

        public  List<PaymentMap> Payments { get; set; } = new List<PaymentMap>();
    }
}
