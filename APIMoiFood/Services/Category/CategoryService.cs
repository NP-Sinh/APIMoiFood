using APIMoiFood.Models.DTOs.Category;
using APIMoiFood.Models.Entities;
using APIMoiFood.Models.Mapping;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace APIMoiFood.Services.CategoryService
{
    public interface ICategoryService
    {
        Task<dynamic> Modify(CategoryRequest request, int id);
        Task<dynamic> GetAll();
        Task<dynamic?> GetById(int id);
        Task<dynamic> Delete(int id);

    }
    public class CategoryService : ICategoryService
    {
        private MoiFoodDBContext _context;
        private readonly IMapper _mapper;
        public CategoryService(MoiFoodDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<dynamic> GetAll()
        {
            var data = await _context.Categories.Where(c => c.IsActive == false)
                .Select(c => new
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description
                })
                .ToListAsync();
            return data;
        }

        public async Task<dynamic?> GetById(int id)
        {
            var data = await _context.Categories
                .Where(c => c.CategoryId == id)
                .Select(c => new
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Description = c.Description
                }).FirstOrDefaultAsync();

            if(data == null)
            {
                return new
                {
                    StatusCode = 404,
                    Message = "Danh mục không tồn tại"
                };
            }

            return data;
        }
        public async Task<dynamic> Modify(CategoryRequest request, int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);

                if (category != null)
                {
                    category.Name = request.Name;
                    category.Description = request.Description;

                    await _context.SaveChangesAsync();

                    return new
                    {
                        StatusCode = 200,
                        Message = "Cập nhật danh mục thành công",
                        Data = _mapper.Map<CategoryMap>(category)
                    };
                }
                else
                {
                    var newCategory = _mapper.Map<Category>(request);
                    newCategory.IsActive = false;
                    _context.Categories.Add(newCategory);
                    await _context.SaveChangesAsync();

                    return new
                    {
                        StatusCode = 201,
                        Message = "Tạo mới danh mục thành công",
                        Data = _mapper.Map<CategoryMap>(newCategory)
                    };
                }
            }
            catch (Exception ex)
            {
                return new
                {
                    StatusCode = 500,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }
        public async Task<dynamic> Delete(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);

                if (category == null)
                {
                    return new
                    {
                        StatusCode = 404,
                        Message = "Danh mục không tồn tại"
                    };
                }
                category.IsActive = true;
                
                await _context.SaveChangesAsync();

                return new
                {
                    StatusCode = 200,
                    Message = "Xóa danh mục thành công",
                };
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
