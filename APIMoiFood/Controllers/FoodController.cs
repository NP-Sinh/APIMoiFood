using APIMoiFood.Models.DTOs.Food;
using APIMoiFood.Services.FoodService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIMoiFood.Controllers
{
    [Route("moifood/[controller]")]
    [ApiController]
    public class FoodController : Controller
    {
        private readonly IFoodService _foodService;
        public FoodController(IFoodService foodService)
        {
            _foodService = foodService;
        }
        [HttpPost("modify")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Modify([FromBody] FoodRequest request, int id)
        {
            var result = await _foodService.Modify(request, id);
            return Ok(result);
        }
    }
}
