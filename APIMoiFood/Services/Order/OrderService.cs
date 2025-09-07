using APIMoiFood.Models.DTOs.Order;
using APIMoiFood.Models.Entities;
using APIMoiFood.Models.Mapping;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace APIMoiFood.Services.OrderService
{
    public interface IOrderService
    {
        Task<dynamic> CreateOrder(int userId, OrderRequest request);
        Task<dynamic> GetOrdersByUserId(int userId);
        Task<dynamic> GetOrderDetails(int userId, int orderId);
    }
    public class OrderService : IOrderService
    {
        public readonly MoiFoodDBContext _context;
        public readonly IMapper _mapper;
        public OrderService(MoiFoodDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<dynamic> GetOrdersByUserId(int userId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Food)
                .Include(o => o.Payments)
                .ThenInclude(p => p.Method)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                
                .ToListAsync();

            return new
            {
                StatusCode = 200,
                Message = "Thành công",
                Oder = _mapper.Map<List<OrderMap>>(orders),
            };
        }
        public async Task<dynamic> GetOrderDetails(int userId, int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Food)
                .Include(o => o.Payments)
                .ThenInclude(p => p.Method)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.OrderId == orderId);
            
            return new
            {
                StatusCode = 200,
                Message = "Thành công",
                Order = _mapper.Map<OrderMap>(order)
            };
        }
        public async Task<dynamic> CreateOrder(int userId, OrderRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);

                decimal totalAmount = 0;
                foreach (var item in request.OrderItems)
                {
                    var food = await _context.Foods.FindAsync(item.FoodId);
                    totalAmount += food.Price * item.Quantity;
                }

                var order = new Order
                {
                    UserId = userId,
                    DeliveryAddress = request.DeliveryAddress,
                    Note = request.Note,
                    TotalAmount = totalAmount,
                    OrderStatus = "pending",
                    PaymentStatus = "pending",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                };

                foreach (var item in request.OrderItems)
                {
                    var food = await _context.Foods.FindAsync(item.FoodId);
                    order.OrderItems.Add(new OrderItem
                    {
                        FoodId = item.FoodId,
                        Quantity = item.Quantity,
                        Price = food.Price,
                        Note = item.Note
                    });
                }

                var payment = new Payment
                {
                    MethodId = request.PaymentMethodId,
                    Amount = totalAmount,
                    PaymentStatus = "pending",
                    CreatedAt = DateTime.Now,
                };
                order.Payments.Add(payment);

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                return new
                {
                    StatusCode = 200,
                    Message = "Đặt hàng thành công",
                    Order = _mapper.Map<OrderMap>(order)
                };

            } 
            catch (Exception ex)
            { 
                return new
                {
                    StatusCode = 500,
                    Message = "Lỗi server: " + ex.Message
                };
            }
        }

    }
}
