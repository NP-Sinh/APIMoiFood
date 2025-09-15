namespace APIMoiFood.Models.Entities
{
    public class GlobalNotification
    {
        public int GlobalNotificationId { get; set; }

        public string Title { get; set; } = null!;

        public string Message { get; set; } = null!;

        public string? NotificationType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
