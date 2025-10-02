using APIMoiFood.Models.DTOs.ReviewRequest;
using APIMoiFood.Models.Entities;
using APIMoiFood.Services.ReviewService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APIMoiFood.Controllers
{
    [ApiController]
    [Route("moifood/[controller]")]
    public class ReviewController : Controller
    {
        public readonly IReviewService _reviewService;
        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }
        [HttpPost("modify")]
        public async Task<IActionResult> Modify([FromBody] ReviewRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _reviewService.Modify(userId, request);
            return Ok(result);
        }
        [HttpGet("get-history-by-user")]
        public async Task<IActionResult> GetHistoryByUser()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _reviewService.GetHistoryByUserId(userId);
            return Ok(result);
        }
        [HttpPost("delete-review")]
        public async Task<IActionResult> Delete(int reviewId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _reviewService.Delete(userId, reviewId);
            return Ok(result);
        }
        [HttpGet("get-all-review")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllReview()
        {
            var result = await _reviewService.GetReviewAll();
            return Ok(result);
        }
        [HttpGet("get-review-by-food-user")]
        public async Task<IActionResult> GetReviewByFoodUser(int foodId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _reviewService.GetReviewByFoodUserId(userId, foodId);
            return Ok(result);
        }
        [HttpPost("delete-by-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteByAdmin(int reviewId)
        {
            var result = await _reviewService.DeleteByAdmin(reviewId);
            return Ok(result);
        }
        [HttpGet("filter-reviews")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FilterReviews(int? foodId, int? userId, int? minRating, int? maxRating, DateTime? fromDate, DateTime? toDate)
        {
            var result = await _reviewService.FilterReviews(foodId, userId, minRating, maxRating, fromDate, toDate);
            return Ok(result);
        }
    }
}
