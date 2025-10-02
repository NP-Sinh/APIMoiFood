using APIMoiFood.Services.Statistics.StatisticsService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APIMoiFood.Controllers
{
    [Route("moifood/[controller]")]
    [ApiController]
    [Authorize]
    public class StatisticsController : Controller
    {
        private readonly IStatisticsService _statisticsService;
        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }
        [HttpGet("revenue")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRevenue(DateTime? fromDate, DateTime? toDate, string groupBy)
        {
            var result = await _statisticsService.GetRevenueAsync(fromDate, toDate, groupBy);
            return Ok(result);
        }
        [HttpGet("order-count")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrderCount(DateTime? fromDate, DateTime? toDate, string groupBy)
        {
            var result = await _statisticsService.GetOrderCountAsync(fromDate, toDate, groupBy);
            return Ok(result);
        }
        [HttpGet("food-orders")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetFoodOrderStats(int top, DateTime? fromDate, DateTime? toDate)
        {
            var result = await _statisticsService.GetFoodOrderStatsAsync(top, fromDate, toDate);
            return Ok(result);
        }
        [HttpGet("user-spending")]
        public async Task<IActionResult> GetUserSpending(DateTime? fromDate, DateTime? toDate, string groupBy)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _statisticsService.GetUserSpendingAsync(userId, fromDate, toDate, groupBy);
            return Ok(result);
        }
    }
}
