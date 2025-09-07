using APIMoiFood.Models.DTOs.Order;
using APIMoiFood.Services.OrderService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APIMoiFood.Controllers
{
    [Route("moifood/[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _orderService.CreateOrder(userId, request);
            return Ok(result);
        }
        [HttpGet("get-orders")]
        public async Task<IActionResult> GetOrders()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _orderService.GetOrdersByUserId(userId);
            return Ok(result);
        }
        [HttpGet("get-order-details")]
        public async Task<IActionResult> GetOrderDetails([FromQuery] int orderId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _orderService.GetOrderDetails(userId, orderId);
            return Ok(result);
        }

    }
}
