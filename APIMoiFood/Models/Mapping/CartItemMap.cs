namespace APIMoiFood.Models.Mapping
{
    public class CartItemMap
    {
        public int CartItemId { get; set; }
        public int FoodId { get; set; }
        public string? FoodName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
    }
}
