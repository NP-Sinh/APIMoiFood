using APIMoiFood.Models.DTOs.Food;
using APIMoiFood.Models.Entities;
using APIMoiFood.Models.Mapping;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace APIMoiFood.Services.FoodService
{
    public interface IFoodService
    {
        Task<dynamic> Modify(FoodRequest request, int id);
        Task<dynamic> GetAll(bool? isAvailable, bool? isActive);
        Task<dynamic?> GetById(int id);
        Task<dynamic> SetActiveStatus(int id, bool isActive);
        Task<dynamic> SetAvailableStatus(int id, bool isAvailable);
        Task<dynamic> Delete(int id, bool isActive);

    }
    public class FoodService : IFoodService
    {
        private readonly MoiFoodDBContext _context;
        private readonly IMapper _mapper;
        public FoodService(MoiFoodDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<dynamic> GetAll(bool? isAvailable, bool? isActive)
        {
            var query = _context.Foods.AsQueryable();

            if (isAvailable.HasValue)
            {
                query = query.Where(f => f.IsAvailable == isAvailable.Value);
            }
            if (isActive.HasValue)
            {
                query = query.Where(f => f.IsActive == isActive.Value);
            }

            return await query
                .Select(f => new
                {
                    f.FoodId,
                    f.Name,
                    f.Description,
                    f.Price,
                    f.ImageUrl,
                    Category = f.Category != null ? new
                    {
                        f.Category.CategoryId,
                        f.Category.Name
                    } : null,
                    f.IsAvailable,
                    f.IsActive,
                    f.CreatedAt,
                    f.UpdatedAt
                })
                .ToListAsync();
        }
        public async Task<dynamic?> GetById(int id)
        {
            var data = await _context.Foods
                .Where(f => f.FoodId == id && f.IsActive == false)
                .Select(f => new
                {
                    f.FoodId,
                    f.Name,
                    f.Description,
                    f.Price,
                    f.ImageUrl,
                    Category = f.Category != null ? new
                    {
                        f.Category.CategoryId,
                        f.Category.Name
                    } : null,
                    f.IsAvailable,
                    f.IsActive,
                    f.CreatedAt,
                    f.UpdatedAt
                }).FirstOrDefaultAsync();
            return data;
        }
        public async Task<dynamic> Modify(FoodRequest request, int id)
        {
            try
            { 
                var data = await _context.Foods
                    .Include(f => f.Category)
                    .FirstOrDefaultAsync(f => f.FoodId == id);

                if(data != null)
                {
                    _mapper.Map(request, data);
                    data.UpdatedAt = DateTime.Now;

                    await _context.SaveChangesAsync();

                    return new
                    {
                        StatusCode = 200,
                        Message = "Cập nhật thành công",
                        Data = _mapper.Map<FoodMap>(data)
                    };
                }
                else
                {
                    var newFood = _mapper.Map<Food>(request);

                    _context.Foods.Add(newFood);
                    await _context.SaveChangesAsync();
                    return new
                    {
                        StatusCode = 201,
                        Message = "Thành công",
                        Data = _mapper.Map<FoodMap>(newFood)
                    };
                }

            }
            catch (Exception ex)
            {
                return new 
                { 
                    StatusCode = 500,
                    Message = $"Lỗi server: {ex.Message}"
                };
            }
        }
        public async Task<dynamic> SetAvailableStatus(int id, bool isAvailable)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food == null)
            {
                return new { StatusCode = 404, Message = "Món ăn không tồn tại" };
            }
            food.IsAvailable = isAvailable;
            food.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return new
            {
                StatusCode = 200,
                Message = isAvailable ? "Đang bán" : "Ngừng bán"
            };
        }
        public async Task<dynamic> SetActiveStatus(int id, bool isActive)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food == null)
            {
                return new { StatusCode = 404, Message = "Món ăn không tồn tại" };
            }

            food.IsActive = isActive;
            food.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return new
            {
                StatusCode = 200,
                Message = isActive ? "Xoá mềm thành công" : "Khôi phục thành công"
            };
        }

        public async Task<dynamic> Delete(int id, bool isActive)
        {
            var food = await _context.Foods
                .Where(f => f.FoodId == id && f.IsActive == true)
                .FirstOrDefaultAsync();
            if (food == null)
            {
                return new { StatusCode = 404, Message = "Món ăn không tồn tại" };
            }
            _context.Foods.Remove(food);
            await _context.SaveChangesAsync();
            return new { StatusCode = 200, Message = "Xoá món ăn vĩnh viễn thành công" };
        }
    }
}
