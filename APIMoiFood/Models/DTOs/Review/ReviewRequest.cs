namespace APIMoiFood.Models.DTOs.ReviewRequest
{
    public class ReviewRequest
    {
        public int FoodId { get; set; }
        public int Rating { get; set; }      // 1–5
        public string? Comment { get; set; }
    }
}
