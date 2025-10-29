using APIMoiFood.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIMoiFood.Controllers
{
    [Route("moifood/[controller]")]
    [Authorize(Roles = "Admin")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("get-all-user")]
        public async Task<IActionResult> GetAllUser()
        {
            var result = await _userService.getAllUser();
            return Ok(result);
        }
        [HttpGet("get-user-by-id")]
        public async Task<IActionResult> getUserById(int id)
        {
            var result = await _userService.getUserById(id);
            return Ok(result);
        }
        [HttpGet("search-user")]
        public async Task<IActionResult> searchUser(string keyword)
        {
            var result = await _userService.searchUser(keyword);
            return Ok(result);
        }
        [HttpGet("set-active-user")]
        public async Task<IActionResult> setActiveUser(int id, bool isActive)
        {
            var result = await _userService.setActiveUser(id, isActive);
            return Ok(result);
        }
    }
}
