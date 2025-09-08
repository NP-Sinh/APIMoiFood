using APIMoiFood.Models.DTOs.Order;
using APIMoiFood.Models.DTOs.Payment;
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
                Order = _mapper.Map<List<OrderMap>>(orders),
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
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                List<OrderItem> orderItems = new List<OrderItem>();
                decimal totalAmount = 0;

                if (request.FromCart)
                {
                    // đặt món từ giỏ hàng
                    var cart = await _context.Carts
                       .Include(c => c.CartItems)
                       .ThenInclude(ci => ci.Food)
                       .FirstOrDefaultAsync(c => c.UserId == userId);

                    foreach (var ci in cart.CartItems)
                    {
                        totalAmount += ci.Food.Price * ci.Quantity;
                        var oi = new OrderItem
                        {
                            FoodId = ci.FoodId,
                            Quantity = ci.Quantity,
                            Price = ci.Food.Price,

                        };
                        orderItems.Add(oi);
                    }
                    _context.CartItems.RemoveRange(cart.CartItems);
                }
                else
                {
                    // đặt món trực tiếp
                    foreach (var item in request.OrderItems)
                    {
                        var food = await _context.Foods.FindAsync(item.FoodId);
                        totalAmount += food.Price * item.Quantity;

                        var oi = new OrderItem
                        {
                            FoodId = item.FoodId,
                            Quantity = item.Quantity,
                            Price = food.Price,
                            Note = item.Note,

                        };
                        orderItems.Add(oi);
                    }
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
                    OrderItems = orderItems,
                    Payments = new List<Payment>()
                    {
                        new Payment
                        {
                            MethodId = request.PaymentMethodId,
                            Method = await _context.PaymentMethods.FindAsync(request.PaymentMethodId),
                            Amount = totalAmount,
                            PaymentStatus = "pending",
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                        }
                    }
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new
                {
                    StatusCode = 200,
                    Message = "Đặt hàng thành công",
                    Order = _mapper.Map<OrderMap>(order)
                };

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new
                {
                    StatusCode = 500,
                    Message = "Lỗi server: " + ex.Message
                };
            }
        }
    }
}
