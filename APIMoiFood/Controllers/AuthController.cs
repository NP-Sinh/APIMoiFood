using APIMoiFood.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace APIMoiFood.Controllers
{
    [Route("moifood/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _emailService;

        public AuthController(IAuthService authService, IEmailService emailService)
        {
            _authService = authService;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var rs = await _authService.Register(request);
            if (rs == null)
                return BadRequest("Username hoặc Email đã tồn tại!");

            return Ok(rs);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var authResponse = await _authService.Login(request);
            if (authResponse == null)
                return Unauthorized("Sai tài khoản hoặc mật khẩu!");

            return Ok(authResponse);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var result = await _authService.ForgotPasswordAsync(request.Email);
            if (!result) return BadRequest("Email không tồn tại");
            return Ok(new { message = "OTP đặt lại mật khẩu đã gửi về email" });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var isValid = await _authService.VerifyOtpAsync(request.Otp);
            if (!isValid)
                return BadRequest("OTP không hợp lệ hoặc đã hết hạn");

            return Ok(new { message = "Xác thực OTP thành công." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest("Mật khẩu nhập lại không khớp");

            var result = await _authService.ResetPasswordAsync(request.NewPassword);
            if (!result) return BadRequest("Bạn chưa xác thực OTP hoặc OTP đã hết hạn");

            return Ok( new { message = "Đặt lại mật khẩu thành công" });
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ForgotPasswordRequest email)
        {
            var result = await _authService.ResendOtp(email.Email);
            if (result == null || result == false)
                return BadRequest(new { message = "Email không tồn tại hoặc không gửi được OTP" });

            return Ok(new { message = "Mã OTP mới đã được gửi" });
        }

    }
    public class RegisterRequest
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? FullName { get; set; }
    }

    public class LoginRequest
    {
        public string UsernameOrEmail { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime Expiration { get; set; }
    }
    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = null!;
    }
    public class ResetPasswordRequest
    {
        public string NewPassword { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
    }

    public class VerifyOtpRequest
    {
        public string Otp { get; set; } = null!;
    }
}
