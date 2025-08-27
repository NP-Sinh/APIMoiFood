using APIMoiFood.Services;
using Microsoft.AspNetCore.Mvc;

namespace APIMoiFood.Controllers
{
    [Route("moifood/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var user = await _authService.Register(request);
            if (user == null)
                return BadRequest("Username hoặc Email đã tồn tại!");

            return Ok(new
            {
                message = "Đăng ký thành công",
                user.UserId,
                user.Username,
                user.Email
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var authResponse = await _authService.Login(request);
            if (authResponse == null)
                return Unauthorized("Sai tài khoản hoặc mật khẩu!");

            return Ok(authResponse);
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
}
