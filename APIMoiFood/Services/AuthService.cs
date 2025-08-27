using APIMoiFood.Controllers;
using APIMoiFood.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace APIMoiFood.Services
{
    public interface IAuthService
    {
        Task<dynamic> Register(RegisterRequest request);
        Task<dynamic> Login(LoginRequest request);
    }

    public class AuthService : IAuthService
    {
        private readonly MoiFoodDBContext _context;
        private readonly IConfiguration _config;

        public AuthService(MoiFoodDBContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<dynamic> Register(RegisterRequest request)
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
            return user;
        }

        public async Task<dynamic> Login(LoginRequest request)
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

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
