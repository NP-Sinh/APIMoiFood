using APIMoiFood.Models.DTOs.Auth;
using APIMoiFood.Models.Entities;
using APIMoiFood.Models.Mapping;
using APIMoiFood.Services.EmailService;
using APIMoiFood.Services.JwtService;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace APIMoiFood.Services.AuthService
{
    public interface IAuthService
    {
        Task<dynamic?> Register(RegisterRequest request);
        Task<dynamic?> Login(LoginRequest request);
        Task<dynamic?> Logout(int userId, LogoutRequest request);
        Task<dynamic?> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<dynamic?> VerifyOtpAsync(VerifyOtpRequest request);
        Task<dynamic?> ResetPasswordAsync(ResetPasswordRequest request);
        Task<dynamic?> ResendOtp(ForgotPasswordRequest request);
    }

    public class AuthService : IAuthService
    {
        private readonly MoiFoodDBContext _context;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        private readonly IJwtService _jwtService;
        private readonly IMemoryCache _cache;
        private readonly IMapper _mapper;

        public AuthService(
            MoiFoodDBContext context,
            IConfiguration config,
            IEmailService emailService,
            IJwtService jwtService,
            IMemoryCache cache,
            IMapper mapper)
        {
            _context = context;
            _config = config;
            _emailService = emailService;
            _jwtService = jwtService;
            _cache = cache;
            _mapper = mapper;
        }

        public async Task<dynamic?> Register(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username || u.Email == request.Email))
                return null;

            var user = _mapper.Map<User>(request);
            user.PasswordHash = CommonServices.HashPassword(request.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new
            {
                statusCode = 200,
                message = "Đăng ký thành công",
                Data = _mapper.Map<UserMap>(user)
            };
        }

        public async Task<dynamic?> Login(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Username == request.UsernameOrEmail ||
                    u.Email == request.UsernameOrEmail);

            if (user == null || !CommonServices.VerifyPassword(request.Password, user.PasswordHash))
                return null;

            var jwtToken = _jwtService.GenerateToken(user.UserId, user.Username, user.Role ?? "User");
            var refreshToken = _jwtService.GenerateRefreshToken();
            var refreshEntity = AddRefreshToken(user.UserId, refreshToken);

            await _context.SaveChangesAsync();

            return new
            {
                Token = jwtToken,
                TokenExpiry = DateTime.UtcNow.AddHours(24),
                RefreshToken = refreshEntity.RefreshToken1,
                RefreshTokenExpiry = refreshEntity.ExpiryDate
            };
        }

        public async Task<dynamic?> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return new { statusCode = 400, message = "Người dùng không tồn tại" };
            }

            await SendOtpAsync(request.Email, "Mã OTP đặt lại mật khẩu");
            return new { statusCode = 200, message = "OTP đặt lại mật khẩu đã gửi về email" };
        }

        public async Task<dynamic?> VerifyOtpAsync(VerifyOtpRequest request)
        {
            var cacheKey = $"otp_{request.Email}";
            if (!_cache.TryGetValue(cacheKey, out string? cachedOtp) || cachedOtp != request.Otp)
            {
                return new { statusCode = 400, message = "OTP không hợp lệ hoặc đã hết hạn" };
            }

            _cache.Set($"verified_{request.Email}", request.Email, TimeSpan.FromMinutes(5));
            _cache.Remove(cacheKey);

            return new { statusCode = 200, message = "Xác thực OTP thành công." };
        }

        public async Task<dynamic?> ResetPasswordAsync(ResetPasswordRequest request)
        {
            if (!_cache.TryGetValue($"verified_{request.Email}", out string? email))
            {
                return new { statusCode = 400, message = "Bạn chưa xác thực OTP hoặc OTP đã hết hạn" };
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return new { statusCode = 400, message = "Người dùng không tồn tại" };
            }

            user.PasswordHash = CommonServices.HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();

            _cache.Remove($"verified_{request.Email}");

            return new { statusCode = 200, message = "Đặt lại mật khẩu thành công." };
        }

        public async Task<dynamic?> ResendOtp(ForgotPasswordRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return new { statusCode = 400, message = "Người dùng không tồn tại" };
            }

            await SendOtpAsync(request.Email, "Mã OTP mới");
            return new { statusCode = 200, message = "OTP mới đã được gửi" };
        }

        public async Task<dynamic?> Logout(int userId, LogoutRequest request)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.UserId == userId && rt.RefreshToken1 == request.RefreshToken);

            if (token == null)
            {
                return new { statusCode = 400, message = "Token không hợp lệ" };
            }

            token.IsRevoked = true;
            await _context.SaveChangesAsync();

            return new { statusCode = 200, message = "Đăng xuất thành công" };
        }

        private async Task SendOtpAsync(string email, string subject)
        {
            var otp = CommonServices.GenerateOTP(8);
            _cache.Set($"otp_{email}", otp, TimeSpan.FromMinutes(5));

            await _emailService.SendEmailAsync(email, subject,
                $"<p>Mã OTP của bạn là: <b>{otp}</b><br/>Có hiệu lực trong 5 phút.</p>");
        }

        private RefreshToken AddRefreshToken(int userId, string refreshToken)
        {
            var entity = new RefreshToken
            {
                UserId = userId,
                RefreshToken1 = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };
            _context.RefreshTokens.Add(entity);
            return entity;
        }
    }
}
