using APIMoiFood.Models.DTOs.Profile;
using APIMoiFood.Services.ProfileService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
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
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(1024 * 1024 * 100)]
        public async Task<IActionResult> UpdateProfileAsync([FromForm] UpdateProfileRequest request, IFormFile? avatarFile)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var updatedUser = await _profileService.UpdateProfile(userId, request, avatarFile);
            return Ok(updatedUser);
        }
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangePasswordRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _profileService.ChangePassword(userId, request);
            
            return Ok(result);
        }
        [HttpPost("upload-avatar")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(1024 * 1024 * 100)]
        public async Task<IActionResult> UploadAvatar([FromForm] UploadAvatarRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _profileService.UploadAvatar(userId, request.File);
            return Ok(result);
        }



    }
}
