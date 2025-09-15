using APIMoiFood.Models.Entities;
using APIMoiFood.Services.NotificationService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APIMoiFood.Controllers
{
    [ApiController]
    [Route("moifood/[controller]")]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        [HttpGet]
        public async Task<IActionResult> GetUserNotifications(bool? isRead)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _notificationService.GetUserNotifications(userId, isRead);
            return Ok(result);
        }
        [HttpPost("mark-as-read")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _notificationService.MarkAsRead(userId, notificationId);
            return Ok(result);
        }
        [HttpPost("mark-all-as-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _notificationService.MarkAllAsRead(userId);
            return Ok(result);
        }

        [HttpPost("admin/send-to-all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendNotificationToAllUser([FromBody] GlobalNotification request)
        {
            var result = await _notificationService.SendGlobalNotification(request.Title, request.Message, request.Type);
            return Ok(result);
        }
        [HttpPost("admin/send-to-user")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendNotificationToUser([FromBody] Notification request)
        {
            var result = await _notificationService.SendNotification(request.UserId, request.Title, request.Message, request.Type);
            return Ok(result);
        }
        [HttpPost("admin/delete")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _notificationService.Delete(id);
            return Ok(result);
        }
    }
    public class GlobalNotification
    {
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string Type { get; set; } = null!;
    }
    public class Notification
    {
        public int UserId { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string Type { get; set; } = null!;
    }
}
