using APIMoiFood.Services.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace APIMoiFood.Controllers
{
    [Route("moifood/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;
        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }
        [HttpGet]
        public async Task<IActionResult> GetProfileAsync()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _profileService.GetProfile(userId);

            return Ok(user);

        }

        [HttpPost("update-profile")]
        public async Task<IActionResult> UpdateProfileAsync([FromBody] UpdateProfileRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var updatedUser = await _profileService.UpdateProfile(userId, request);
            return Ok(updatedUser);
        }
    }
    public class UpdateProfileRequest
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Avatar { get; set; }
        public string? Address { get; set; }
    }
}
