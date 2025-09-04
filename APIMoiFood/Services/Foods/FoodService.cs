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

        public async Task<dynamic> Modify(FoodRequest request, int id)
        {
            try
            { 
                var data = await _context.Foods
                    .Include(f => f.Category)
                    .FirstOrDefaultAsync(f => f.FoodId == id);

                if(data != null)
                {
                    data.Name = request.Name;
                    data.Description = request.Description;
                    data.Price = request.Price;
                    data.ImageUrl = request.ImageUrl;
                    data.CategoryId = request.CategoryId;
                    data.IsAvailable = request.IsAvailable;
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
                    newFood.IsAvailable = true;
                    newFood.CreatedAt = DateTime.Now;
                    newFood.UpdatedAt = DateTime.Now;
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
    }
}
