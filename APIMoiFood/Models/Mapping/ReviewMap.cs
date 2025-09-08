namespace APIMoiFood.Models.Mapping
{
    public class ReviewMap
    {
        public int ReviewId { get; set; }

        public int? UserId { get; set; }

        public int? FoodId { get; set; }

        public int? Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
