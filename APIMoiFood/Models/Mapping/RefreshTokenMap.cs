namespace APIMoiFood.Models.Mapping
{
    public class RefreshTokenMap
    {
        public int TokenId { get; set; }

        public int? UserId { get; set; }

        public string RefreshToken1 { get; set; } = null!;

        public DateTime ExpiryDate { get; set; }

        public bool? IsRevoked { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
