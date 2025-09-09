namespace APIMoiFood.Models.Mapping
{
    public class OrderItemMap
    {
        public int OrderItemId { get; set; }

        public int? OrderId { get; set; }

        public int? FoodId { get; set; }
        // FoodName
        public string? FoodName { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public string? Note { get; set; }
    }
}
