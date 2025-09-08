using APIMoiFood.Models.Entities;

namespace APIMoiFood.Models.Mapping
{
    public partial class CategoryMap
    {
        public int CategoryId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }
        public bool? IsActive { get; set; }

        public List<Food> Foods { get; set; } = new List<Food>();
    }
}
