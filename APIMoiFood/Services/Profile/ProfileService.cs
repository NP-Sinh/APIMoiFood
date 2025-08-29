using APIMoiFood.Controllers;
using APIMoiFood.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace APIMoiFood.Services.Profile
{
    public interface IProfileService
    {
        Task<dynamic?> GetProfile(int userId);
        Task<dynamic?> UpdateProfile(int userId, UpdateProfileRequest request);
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
                    UsserId = x.UserId,
                    FullName = x.FullName,
                    Email = x.Email,
                    Phone = x.Phone,
                    Address = x.Address,
                    Avatar = x.Avatar,
                })
                .FirstOrDefaultAsync(x => x.UsserId == userId);
            return data;
        }

        public async Task<dynamic?> UpdateProfile(int userId, UpdateProfileRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            user.FullName = request.FullName ?? user.FullName;
            user.Phone = request.Phone ?? user.Phone;
            user.Avatar = request.Avatar ?? user.Avatar;
            user.Address = request.Address ?? user.Address;
            user.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return user;

        }
    }
}
