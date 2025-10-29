using APIMoiFood.Models.Entities;
using APIMoiFood.Services.NotificationService;
using AutoMapper;
using Azure.Core;
using Microsoft.EntityFrameworkCore;

namespace APIMoiFood.Services.UserService
{

    public interface IUserService
    {
        Task<dynamic> getAllUser();
        Task<dynamic> getUserById(int id);

        Task<dynamic> setActiveUser(int id, bool isActive);

        Task<dynamic> searchUser(string keyword);

    }
    public class UserService : IUserService
    {
        private readonly MoiFoodDBContext _context;
        private readonly IMapper _mapper;
        public UserService(MoiFoodDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper; 
        }

        public async Task<dynamic> getAllUser()
        {
            var query = await _context.Users
                   .Select(x => new
                   {
                       x.UserId,
                       x.Username,
                       x.FullName,
                       x.Avatar,
                       x.Email,
                       x.Phone,
                       x.Address,
                       x.IsActive,
                       x.Role,
                       x.CreatedAt,
                       x.UpdatedAt,
                   })
                   .ToListAsync();
            return query;
        }

        public async Task<dynamic> getUserById(int id)
        {
            var query = await _context.Users
                .Where(x => x.UserId == id)
                .Select(x => new
                {
                    x.UserId,
                    x.Username,
                    x.FullName,
                    x.Avatar,
                    x.Email,
                    x.Phone,
                    x.Address,
                    x.IsActive,
                    x.Role,
                    x.CreatedAt,
                    x.UpdatedAt,
                }).FirstOrDefaultAsync();

            return query;
        }

        public async Task<dynamic> searchUser(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return await _context.Users
                    .AsNoTracking()
                    .Select(x => new
                    {
                        x.UserId,
                        x.Username,
                        x.FullName,
                        x.Avatar,
                        x.Email,
                        x.Phone,
                        x.Address,
                        x.IsActive,
                        x.Role,
                        x.CreatedAt,
                        x.UpdatedAt,
                    })
                    .ToListAsync();
            }
            keyword = CommonServices.RemoveDiacritics(keyword.Trim().ToLower());

            var users = await _context.Users
                .AsNoTracking()
                .Select(x => new
                {
                    x.UserId,
                    x.Username,
                    x.FullName,
                    x.Avatar,
                    x.Email,
                    x.Phone,
                    x.Address,
                    x.IsActive,
                    x.Role,
                    x.CreatedAt,
                    x.UpdatedAt,
                })
                .ToListAsync();

            return users
                 .Where(f =>
                     CommonServices.RemoveDiacritics((f.FullName ?? "").ToLower()).Contains(keyword) ||
                     CommonServices.RemoveDiacritics((f.Phone ?? "").ToLower()).Contains(keyword) ||
                     CommonServices.RemoveDiacritics((f.Address ?? "").ToLower()).Contains(keyword) ||
                     CommonServices.RemoveDiacritics((f.Email ?? "").ToLower()).Contains(keyword)
                 )
                 .ToList();
        }

        public async Task<dynamic> setActiveUser(int id, bool isActive)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.UserId == id);
                user.IsActive = isActive;
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new 
                {

                    StatusCode = 200,
                    message = "Thành công",
                    data = user
                };
            }
            catch (Exception e)
            {
                return new
                {
                    StatusCode = 500,
                    message = "Thất bại",
                };
            }
        }
    }
}
