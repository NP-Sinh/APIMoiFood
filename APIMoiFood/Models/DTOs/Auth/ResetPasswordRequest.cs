namespace APIMoiFood.Models.DTOs.Auth
{
    public class ResetPasswordRequest
    {
        public string NewPassword { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
    }
}
