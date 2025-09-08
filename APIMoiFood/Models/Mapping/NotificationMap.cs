namespace APIMoiFood.Models.Mapping
{
    public class NotificationMap
    {
        public int NotificationId { get; set; }

        public int? UserId { get; set; }

        public string Title { get; set; } = null!;

        public string Message { get; set; } = null!;

        public string? NotificationType { get; set; }

        public bool? IsRead { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
