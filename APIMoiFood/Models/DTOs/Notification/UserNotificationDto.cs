namespace APIMoiFood.Models.DTOs.Notification
{
    public class UserNotificationDto
    {
        public int NotificationId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string NotificationType { get; set; }
        public bool? IsRead { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
