using APIMoiFood.Models.DTOs.Category;
using APIMoiFood.Services.CategoryService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIMoiFood.Controllers
{
    [Route("moifood/[controller]")]
    [ApiController]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _categoryService.GetAll();
            return Ok(result);
        }

        [HttpPost("getById")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _categoryService.GetById(id);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("modify")]
        public async Task<IActionResult> Modify(int id, [FromBody] CategoryRequest request)
        {
            var rs = await _categoryService.Modify(request, id);
            return Ok(rs);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("delete-Category")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var rs = await _categoryService.Delete(id);
            return Ok(rs);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("restore-Category")]
        public async Task<IActionResult> RestoreCategory(int id)
        {
            var rs = await _categoryService.Restore(id);
            return Ok(rs);
        }

    }
}
