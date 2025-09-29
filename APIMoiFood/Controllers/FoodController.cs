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

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll(bool? isAvailable, bool? isActive)
        {
            var result = await _foodService.GetAll(isAvailable, isActive);
            return Ok(result);
        }

        [HttpPost("modify")]
        [Consumes("multipart/form-data")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Modify([FromForm] FoodRequest request, [FromQuery] int id, IFormFile imageUrl)
        {
            var result = await _foodService.Modify(request, id, imageUrl);
            return Ok(result);
        }
        [HttpPost("getbyid")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _foodService.GetById(id);

            return Ok(result);
        }
        [HttpPost("set-active-status")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetActiveStatus(int id, bool isActive)
        {
            var result = await _foodService.SetActiveStatus(id, isActive);
            return Ok(result);
        }

        [HttpPost("set-available-status")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetAvailableStatus(int id, bool isAvailable)
        {
            var result = await _foodService.SetAvailableStatus(id, isAvailable);
            return Ok(result);
        }

        [HttpPost("delete")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id, bool isActive)
        {
            var result = await _foodService.Delete(id, isActive);
            return Ok(result);
        }  
    }
}
