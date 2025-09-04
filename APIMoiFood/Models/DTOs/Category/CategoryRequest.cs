namespace APIMoiFood.Models.DTOs.Category
{
    public partial class CategoryRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
