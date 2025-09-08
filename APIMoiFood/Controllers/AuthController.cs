using APIMoiFood.Models.DTOs.Auth;
using APIMoiFood.Models.Entities;
using APIMoiFood.Services.AuthService;
using APIMoiFood.Services.EmailService;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
            var result = await _authService.ForgotPasswordAsync(request);

            return Ok(result);
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            var isValid = await _authService.VerifyOtpAsync(request);

            return Ok(isValid);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest("Mật khẩu nhập lại không khớp");

            var result = await _authService.ResetPasswordAsync(request);

            return Ok(result);
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ForgotPasswordRequest email)
        {
            var result = await _authService.ResendOtp(email);

            return Ok(result);
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var result = await _authService.Logout(userId, request);

            return Ok(result);
        }

    }
}
