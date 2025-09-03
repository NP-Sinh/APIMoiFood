using System.ComponentModel.DataAnnotations;

namespace APIMoiFood.Models.DTOs.Profile
{
    public class UploadAvatarRequest
    {
        [Required]
        public IFormFile File { get; set; }
    }
}
