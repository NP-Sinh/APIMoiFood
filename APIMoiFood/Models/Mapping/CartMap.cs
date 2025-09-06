namespace APIMoiFood.Models.Mapping
{
    public class CartMap
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public List<CartItemMap> Items { get; set; } = new List<CartItemMap>();
        public decimal TotalPrice => Items.Sum(i => i.Total);
    }
}
