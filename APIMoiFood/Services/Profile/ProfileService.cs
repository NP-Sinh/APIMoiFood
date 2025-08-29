using APIMoiFood.Controllers;
using APIMoiFood.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace APIMoiFood.Services.Profile
{
    public interface IProfileService
    {
        Task<dynamic?> GetProfile(int userId);
        Task<dynamic?> UpdateProfile(int userId, UpdateProfileRequest request);
        Task<dynamic?> ChangePassword(int userId, ChangePasswordRequest request);
        Task<dynamic?> UploadAvatar(int userId, IFormFile file);
    }
    public class ProfileService : IProfileService
    {
        private readonly MoiFoodDBContext _context;
        public ProfileService(MoiFoodDBContext context)
        {
            _context = context;
        }

        public async Task<dynamic?> GetProfile(int userId)
        {
            var data = await _context.Users
                .Select(x => new
                {
                    UserId = x.UserId,
                    FullName = x.FullName,
                    Email = x.Email,
                    Phone = x.Phone,
                    Address = x.Address,
                    Avatar = x.Avatar,
                })
                .FirstOrDefaultAsync(x => x.UserId == userId);
            return data;
        }

        public async Task<dynamic?> UpdateProfile(int userId, UpdateProfileRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            user.FullName = request.FullName;
            user.Phone = request.Phone;
            user.Avatar = request.Avatar;
            user.Address = request.Address;
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return user;

        }
        public async Task<dynamic?> ChangePassword(int userId, ChangePasswordRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if(!CommonServices.VerifyPassword(request.OldPassword, user.PasswordHash))
                return new { Message = "Mật khẩu cũ không đúng" };

            if (request.NewPassword != request.ConfirmPassword)
                return new { Message = "Mật khẩu xác nhận không khớp" };

            user.PasswordHash = CommonServices.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return new { Message = "Đổi mật khẩu thành công" };
        }

        public async Task<dynamic?> UploadAvatar(int userId, IFormFile file)
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            user.Avatar = $"/avatars/{fileName}";
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return new { Message = "Upload ảnh thành công", AvatarUrl = user.Avatar };
        }
    }
}

