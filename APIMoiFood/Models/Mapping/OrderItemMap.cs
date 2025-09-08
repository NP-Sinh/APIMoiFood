namespace APIMoiFood.Models.Mapping
{
    public class OrderItemMap
    {
        public int FoodId { get; set; }
        public string? FoodName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? Note { get; set; }
    }
}
