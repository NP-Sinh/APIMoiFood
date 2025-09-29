using APIMoiFood.Controllers;
using APIMoiFood.Models.DTOs.Profile;
using APIMoiFood.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace APIMoiFood.Services.ProfileService
{
    public interface IProfileService
    {
        Task<dynamic?> GetProfile(int userId);
        Task<dynamic?> UpdateProfile(int userId, UpdateProfileRequest request, IFormFile? avatarFile);
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
        
        public async Task<dynamic?> UpdateProfile(int userId, UpdateProfileRequest request, IFormFile? avatarFile)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (avatarFile != null)
            {
                if (!string.IsNullOrEmpty(user.Avatar))
                {
                    var oldPhysicalPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        user.Avatar.TrimStart('/'));
                    if (System.IO.File.Exists(oldPhysicalPath))
                        System.IO.File.Delete(oldPhysicalPath);
                }

                var relativePath = await CommonServices.SaveFileAsync(avatarFile, "avatars");
                user.Avatar = relativePath;
            }

            user.FullName = request.FullName;
            user.Phone = request.Phone;
            user.Address = request.Address;
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return new
            {
                UserId = userId,
                FullName = user.FullName,
                Phone = user.Phone,
                Address = user.Address,
                Avatar = user.Avatar,
                UpdatedAt = DateTime.Now,
            };

        }
        public async Task<dynamic?> ChangePassword(int userId, ChangePasswordRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if(!CommonServices.VerifyPassword(request.OldPassword, user.PasswordHash))
                return new 
                {
                    Message = "Mật khẩu cũ không đúng" 
                };

            if (request.NewPassword != request.ConfirmPassword)
                return new 
                { 
                    Message = "Mật khẩu xác nhận không khớp" 
                };

            user.PasswordHash = CommonServices.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return new 
            { 
                Message = "Đổi mật khẩu thành công" 
            };
        }

        public async Task<dynamic?> UploadAvatar(int userId, IFormFile file)
        {
            var relativePath = await CommonServices.SaveFileAsync(file, "avatars");

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if(user == null) return null;

            if (!string.IsNullOrEmpty(user.Avatar))
            {
                var oldPhysicalPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    user.Avatar.TrimStart('/'));
                if (System.IO.File.Exists(oldPhysicalPath))
                {
                    System.IO.File.Delete(oldPhysicalPath);
                }
            }

            user.Avatar = relativePath;
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return new
            {
                StatusCode = 200,
                message = "Upload thành công",
                AvatarUrl = user.Avatar,
            };    
        }
    }
}

