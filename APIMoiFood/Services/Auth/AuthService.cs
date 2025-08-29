using APIMoiFood.Controllers;
using APIMoiFood.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static System.Net.WebRequestMethods;

namespace APIMoiFood.Services.Auth
{
    public interface IAuthService
    {
        Task<dynamic?> Register(RegisterRequest request);
        Task<dynamic?> Login(LoginRequest request);
        Task<dynamic?> ForgotPasswordAsync(string email);
        Task<dynamic?> VerifyOtpAsync(string otp);
        Task<dynamic?> ResetPasswordAsync(string newPassword);
        Task<dynamic?> ResendOtp(string email);
    }

    public class AuthService : IAuthService
    {
        private readonly MoiFoodDBContext _context;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;

        public AuthService(MoiFoodDBContext context, IConfiguration config, IEmailService emailService, IMemoryCache cache)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
            _cache = cache;
        }
        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
        public async Task<dynamic?> Register(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email))
                return null;

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
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

            if (user == null) return null;

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role ?? "User")
                }),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            var refreshToken = GenerateRefreshToken();
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.UserId,
                RefreshToken1 = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshTokenEntity);
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Token = jwtToken,
                RefreshToken = refreshToken,
                Expiration = tokenDescriptor.Expires ?? DateTime.UtcNow.AddMinutes(30)
            };
        }

        public async Task<dynamic?> ForgotPasswordAsync(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null) return false;

            var otp = new Random().Next(10000000, 99999999).ToString();

            // lưu OTP -> Email
            _cache.Set($"otp_{otp}", email, TimeSpan.FromMinutes(5));

            await _emailService.SendEmailAsync(email, "Mã OTP đặt lại mật khẩu",
                $"<p>Mã OTP của bạn là: <b>{otp}</b><br/>Có hiệu lực trong 5 phút.</p>");

            return true;
        }

        public async Task<dynamic?> VerifyOtpAsync(string otp)
        {
            if (!_cache.TryGetValue($"otp_{otp}", out string? email))
                return false;

            // Đánh dấu email đã verify OTP
            _cache.Set("verified_email", email, TimeSpan.FromMinutes(5));

            return true;
        }


        public async Task<dynamic?> ResetPasswordAsync(string newPassword)
        {
            if (!_cache.TryGetValue("verified_email", out string? email))
                return false;

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            _cache.Remove("verified_email");

            return true;
        }

        public async Task<dynamic?> ResendOtp(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null) return false;

            var otp = new Random().Next(10000000, 99999999).ToString();

            _cache.Set($"otp_{otp}", email, TimeSpan.FromMinutes(5));

            await _emailService.SendEmailAsync(email, "Mã OTP mới",
                $"<p>Mã OTP mới của bạn là: <b>{otp}</b><br/>Có hiệu lực trong 5 phút.</p>");

            return true;
        }


    }
}
