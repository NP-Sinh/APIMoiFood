using APIMoiFood.Models.DTOs.Notification;
using APIMoiFood.Models.Entities;
using AutoMapper.Internal;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace APIMoiFood.Services.NotificationService
{
    public class NotificationHub : Hub
    {
        public async Task JoinUserGroup(int userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
    }
    public interface INotificationService
    {
        // User
        Task<dynamic> GetUserNotifications(int userId, bool? isRead);
        Task<dynamic> MarkAsRead(int userId, int notificationId);
        Task<dynamic> MarkAllAsRead(int userId);
        // Admin
        Task<dynamic> GetGlobalNotifications();
        Task<dynamic> GetNotificationsByUserId(int? userId);
        Task<dynamic> SendGlobalNotification(string title, string message, string type);
        Task<dynamic> SendNotification(int userId, string title, string message, string type);
        Task<dynamic> Delete(int id);
    }
    public class NotificationService : INotificationService
    {
        private readonly MoiFoodDBContext _context;

        private readonly IHubContext<NotificationHub> _hub;

        public NotificationService(MoiFoodDBContext context, IHubContext<NotificationHub> hub)
        {
            _context = context;
            _hub = hub;
        }
        public async Task<dynamic> GetUserNotifications(int userId, bool? isRead)
        {
            // Thông báo cá nhân
            var personal = await _context.Notifications
                .Where(n => n.UserId == userId &&
                            (!isRead.HasValue || n.IsRead == isRead.Value))
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new UserNotificationDto
                {
                    NotificationId = n.NotificationId,
                    Title = n.Title,
                    Message = n.Message,
                    NotificationType = n.NotificationType,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                })
                .ToListAsync();

            // Thông báo chung
            var global = await _context.GlobalNotifications
                .OrderByDescending(g => g.CreatedAt)
                .Select(g => new UserNotificationDto
                {
                    NotificationId = g.GlobalNotificationId,
                    Title = g.Title,
                    Message = g.Message,
                    NotificationType = g.NotificationType,
                    IsRead = null,
                    CreatedAt = g.CreatedAt,
                })
                .ToListAsync();

            // Gộp và sắp xếp
            var result = personal
                .Concat(global)                  
                .OrderByDescending(n => n.CreatedAt)
                .ToList();

            return result;
        }
        public async Task<dynamic> MarkAsRead(int userId, int notificationId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return new { Message = "Notification marked as read successfully." };
        }
        public async Task<dynamic> MarkAllAsRead(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && n.IsRead == false)
                .ToListAsync();
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }
            await _context.SaveChangesAsync();
            return new { Message = "All notifications marked as read successfully." };
        }
        // admin
        public async Task<dynamic> GetGlobalNotifications()
        {
            var query = await _context.GlobalNotifications
                .Select(x => new
                {
                    GlobalNotificationId = x.GlobalNotificationId,
                    Title = x.Title,
                    Message = x.Message,
                    NotificationType = x.NotificationType,
                    CreatedAt = x.CreatedAt,
                })
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
            return query;
        }
        public async Task<dynamic> GetNotificationsByUserId(int? userId)
        {
            var query = _context.Notifications.AsQueryable();

            if (userId.HasValue)
            {
                query = query.Where(q => q.UserId == userId);
            }

            var result = await query
                .Select(x => new
                {
                    NotificationId = x.NotificationId,
                    Title = x.Title,
                    UserId = x.UserId,
                    FullName = x.User.FullName,
                    Phone = x.User.Phone,
                    Email = x.User.Email,
                    Message = x.Message,
                    NotificationType = x.NotificationType,
                    IsRead = x.IsRead,
                    CreatedAt = x.CreatedAt,
                })
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return result;
        }

        public async Task<dynamic> SendGlobalNotification(string title, string message, string type)
        {
            var g = new GlobalNotification
            {
                Title = title,
                Message = message,
                NotificationType = type,
                CreatedAt = DateTime.Now,
            };
            await _context.GlobalNotifications.AddAsync(g);
            await _context.SaveChangesAsync();

            return new
            {
                Message = "Notifications sent to all users successfully."
            };
        }
        public async Task<dynamic> SendNotification(int userId, string title, string message, string type)
        {
            var user = await _context.Users.FindAsync(userId);

            var n = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                NotificationType = type,
                IsRead = false,
                CreatedAt = DateTime.Now
            };
            await _context.Notifications.AddAsync(n);
            await _context.SaveChangesAsync();

            // push realtime
            await _hub.Clients.User(userId.ToString())
                      .SendAsync("ReceiveNotification", new
                      {
                          n.NotificationId,
                          n.Title,
                          n.Message,
                          n.NotificationType,
                          n.CreatedAt
                      });

            return new { Message = "Notification sent successfully." };
        }
        public async Task<dynamic> Delete(int id)
        {
            var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationId == id);
            if (notification == null)
            {
                return new { Message = "Notification not found." };
            }
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return new { Message = "Notification deleted successfully." };
        }
    }
}
