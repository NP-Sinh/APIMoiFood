using APIMoiFood.Models.DTOs.ReviewRequest;
using APIMoiFood.Models.Entities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace APIMoiFood.Services.ReviewService
{
    public interface IReviewService
    {
        Task<dynamic> Modify(int userId, ReviewRequest request);
        Task<dynamic> Delete(int userId, int reviewId);
        Task<dynamic> GetHistoryByUserId(int userId);
        Task<dynamic> GetReviewByFoodUserId(int userId, int foodId);
        // Admin
        Task<dynamic> GetReviewAll();
        Task<dynamic> DeleteByAdmin(int reviewId);
        Task<dynamic> FilterReviews(int? foodId, int? userId,int? minRating, int? maxRating, DateTime? fromDate, DateTime? toDate);

    }
    public class ReviewService : IReviewService
    {
        private readonly MoiFoodDBContext _context;
        private readonly IMapper _map;
        public ReviewService(MoiFoodDBContext context, IMapper map)
        {
            _context = context;
            _map = map;
        }
        public async Task<dynamic> GetReviewAll()
        {
            var reviews = await _context.Reviews
                .Select(r => new 
                { 
                    r.ReviewId,
                    r.UserId,
                    Food = new
                    {
                        foodId = r.Food.FoodId,
                        foodName = r.Food.Name,
                    },
                    r.Rating,
                    r.Comment,
                    r.CreatedAt
                })
                .ToListAsync();
            return reviews;
        }
        public async Task<dynamic> GetHistoryByUserId(int userId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.UserId == userId)
                .Select(r => new 
                { 
                    r.ReviewId,
                    r.UserId,
                    Food = new
                    {
                        foodId = r.Food.FoodId,
                        foodName = r.Food.Name,
                    },
                    r.Rating,
                    r.Comment,
                    r.CreatedAt
                })
                .ToListAsync();
            return reviews;
        }
        public async Task<dynamic> GetReviewByFoodUserId(int userId, int foodId)
        {
            var review = await _context.Reviews
                .Where(r => r.UserId == userId && r.FoodId == foodId)
                .Select(r => new 
                { 
                    r.ReviewId,
                    r.UserId,
                    Food = new
                    {
                        foodId = r.Food.FoodId,
                        foodName = r.Food.Name,
                    },
                    r.Rating,
                    r.Comment,
                    r.CreatedAt
                })
                .FirstOrDefaultAsync();
            return review;
        }
        public async Task<dynamic> Delete(int userId, int reviewId)
        {
            var data = await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.ReviewId == reviewId);

            _context.Reviews.Remove(data);
            await _context.SaveChangesAsync();
            
            return new
            {
                StatusCode = 200,
                Message = "Xoá đánh giá thành công",
            };
        }
        public async Task<dynamic> Modify(int userId, ReviewRequest request)
        {
            try
            {
                var data = await _context.Reviews
               .FirstOrDefaultAsync(r => r.UserId == userId && r.FoodId == request.FoodId);

                if (data != null)
                {
                   _map.Map(request, data);
                    data.CreatedAt = DateTime.Now;

                    _context.Reviews.Update(data);
                }
                else
                {
                    var newReview = _map.Map<Review>(request);
                    newReview.UserId = userId;
                    newReview.CreatedAt = DateTime.Now;
                    await _context.Reviews.AddAsync(newReview);
                    data = newReview;
                }
                await _context.SaveChangesAsync();
                return new
                {
                    StatusCode = 200,
                    Message = "Đánh giá thành công",
                    Review = data
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    StatusCode = 500,
                    Message = "Lỗi server: " + ex.Message,
                };
            }
        }

        public async Task<dynamic> DeleteByAdmin(int reviewId)
        {
            var data = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId);
            _context.Reviews.Remove(data);
            await _context.SaveChangesAsync();
            return new
            {
                StatusCode = 200,
                Message = "Xoá đánh giá thành công",
            };
        }

        public async Task<dynamic> FilterReviews(int? foodId, int? userId,
                                         int? minRating, int? maxRating,
                                         DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.Reviews.AsQueryable();

            if (foodId.HasValue)
                query = query.Where(r => r.FoodId == foodId);

            if (userId.HasValue)
                query = query.Where(r => r.UserId == userId);

            if (minRating.HasValue)
                query = query.Where(r => r.Rating >= minRating);

            if (maxRating.HasValue)
                query = query.Where(r => r.Rating <= maxRating);

            if (fromDate.HasValue)
                query = query.Where(r => r.CreatedAt >= fromDate);

            if (toDate.HasValue)
                query = query.Where(r => r.CreatedAt <= toDate);

            var result = await query
                .Include(r => r.Food)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.ReviewId,
                    User = new { r.UserId, r.User!.FullName },
                    Food = new { r.FoodId, r.Food!.Name },
                    r.Rating,
                    r.Comment,
                    r.CreatedAt
                })
                .ToListAsync();

            return result;
        }

    }
}