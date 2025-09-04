namespace APIMoiFood.Models.Mapping
{
    public partial class CategoryMap
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
    }
}
