namespace APIMoiFood.Models.Mapping
{
    public class CartItemMap
    {
        public int CartItemId { get; set; }

        public int? CartId { get; set; }

        public int? FoodId { get; set; }

        public int Quantity { get; set; }
    }
}
