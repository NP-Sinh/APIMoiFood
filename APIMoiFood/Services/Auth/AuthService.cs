using APIMoiFood.Controllers;
using APIMoiFood.Models.DTOs.Auth;
using APIMoiFood.Models.Entities;
using APIMoiFood.Services.EmailService;
using APIMoiFood.Services.JwtService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static System.Net.WebRequestMethods;

namespace APIMoiFood.Services.AuthService
{
    public interface IAuthService
    {
        Task<dynamic?> Register(RegisterRequest request);
        Task<dynamic?> Login(LoginRequest request);
        Task<dynamic?> Logout(int userId, string refreshToken);
        Task<dynamic?> ForgotPasswordAsync(string email);
        Task<dynamic?> VerifyOtpAsync(string email, string otp);
        Task<dynamic?> ResetPasswordAsync(string newPassword);
        Task<dynamic?> ResendOtp(string email);
    }

    public class AuthService : IAuthService
    {
        private readonly MoiFoodDBContext _context;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        private readonly IJwtService _jwtService;
        private readonly IMemoryCache _cache;

        public AuthService(MoiFoodDBContext context, IConfiguration config, IEmailService emailService, IJwtService jwtService, IMemoryCache cache)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
            _jwtService = jwtService;
            _cache = cache;
        }
        public async Task<dynamic?> Register(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email))
                return null;

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = CommonServices.HashPassword(request.Password),
                FullName = request.FullName,
                Role = "User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return new
            {
                message = "Đăng ký thành công",
                user.UserId,
                user.Username,
                user.Email
            };
        }

        public async Task<dynamic?> Login(LoginRequest request)
        {
            var user = await _context.Users
               .FirstOrDefaultAsync(u =>
                   u.Username == request.UsernameOrEmail ||
                   u.Email == request.UsernameOrEmail);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            var response = GenerateAuthResponse(user);
            await _context.SaveChangesAsync();

            return response;
        }

        public async Task<dynamic?> ForgotPasswordAsync(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return new
                {
                    statusCode = 400,
                    message = "Người dùng không tồn tại",
                };
            }

            var otp = CommonServices.GenerateOTP(8);

            // luôn ghi đè OTP theo email
            _cache.Set($"otp_{email}", otp, TimeSpan.FromMinutes(5));

            await _emailService.SendEmailAsync(email, "Mã OTP đặt lại mật khẩu",
                $"<p>Mã OTP của bạn là: <b>{otp}</b><br/>Có hiệu lực trong 5 phút.</p>");

            return new 
            {
                statusCode = 200,
                message = "OTP đặt lại mật khẩu đã gửi về email",
            };
        }

        public async Task<dynamic?> VerifyOtpAsync(string email, string otp)
        {
            if (!_cache.TryGetValue($"otp_{email}", out string? cachedOtp) || cachedOtp != otp)
            {
                return new
                {
                    statusCode = 400,
                    message = "OTP không hợp lệ hoặc đã hết hạn",
                };
            }    

            _cache.Set("verified_email", email, TimeSpan.FromMinutes(5));

            _cache.Remove($"otp_{email}");

            return new
            {
                statusCode = 200,
                message = "Xác thực OTP thành công."
            };
        }

        public async Task<dynamic?> ResetPasswordAsync(string newPassword)
        {
            if (!_cache.TryGetValue("verified_email", out string? email))
            {
                return new
                {
                    statusCode = 400,
                    message = "Bạn chưa xác thực OTP hoặc OTP đã hết hạn",
                };
            }    

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return new
                {
                    statusCode = 400,
                    message = "Người dùng không tồn tại",
                };
            };

            user.PasswordHash = CommonServices.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            _cache.Remove("verified_email");

            return new
            {
                statusCode = 200,
                message = "Đặt lại mật khẩu thành công."
            };
        }

        public async Task<dynamic?> ResendOtp(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return new
                {
                    statusCode = 400,
                    message = "Người dùng không tồn tại",
                };
            }
            ;

            var otp = CommonServices.GenerateOTP(8);

            _cache.Set($"otp_{email}", otp, TimeSpan.FromMinutes(5));

            await _emailService.SendEmailAsync(email, "Mã OTP mới",
                $"<p>Mã OTP mới của bạn là: <b>{otp}</b><br/>Có hiệu lực trong 5 phút.</p>");

            return new 
            { 
                statusCode = 200,
                message = "OTP mới đã được gửi",
            };
        }
        // Logout bằng cách thu hồi refresh token
        public async Task<dynamic?> Logout(int userId, string refreshToken)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.UserId == userId && rt.RefreshToken1 == refreshToken);

            if (token == null)
            {
                return new
                {
                    statusCode = 400,
                    message = "Token không hợp lệ",
                };
            }

            token.IsRevoked = true;
            await _context.SaveChangesAsync();

            return new
            { 
                statusCode = 200,
                message = "Đăng xuất thành công"
            };
        }

        private AuthResponse GenerateAuthResponse(User user)
        {
            var jwtToken = _jwtService.GenerateToken(user.UserId, user.Username, user.Role ?? "User");
            var refreshToken = _jwtService.GenerateRefreshToken();

            var refreshEntity = new RefreshToken
            {
                UserId = user.UserId,
                RefreshToken1 = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };
            _context.RefreshTokens.Add(refreshEntity);

            return new AuthResponse
            {
                Token = jwtToken,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(15)
            };
        }


    }
}
