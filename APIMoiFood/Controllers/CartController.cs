using APIMoiFood.Models.DTOs.Cart;
using APIMoiFood.Services.CartService;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APIMoiFood.Controllers
{
    [ApiController]
    [Route("moifood/[controller]")]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _cartService.GetCart(userId);
            return Ok(result);
        }
        [HttpPost("add-to-cart")]
        public async Task<IActionResult> AddToCart([FromQuery] CartRequest cartRequest ,[FromBody] CartItemRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _cartService.AddToCart(userId, cartRequest, request);
            return Ok(result);
        }
        [HttpPost("update-quantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] CartItemRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _cartService.UpdateQuantity(userId, request);
            return Ok(result);
        }
        [HttpPost("remove-from-cart")]
        public async Task<IActionResult> RemoveFromCart(int foodId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _cartService.RemoveFromCart(userId, foodId);
            return Ok(result);
        }
    }
}
