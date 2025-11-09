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
        Task<dynamic> GetOrdersByUserId(int userId, string orderStatus);
        Task<dynamic> GetOrderDetails(int userId, int orderId);
        Task<dynamic> ConfirmReceived(int userId, int orderId);
        Task<dynamic> CancelOrder(int userId, int orderId);



        // Admin
        Task<dynamic> GetAllOrders(string? status);
        Task<dynamic> GetOrderById(int orderId);
        Task<dynamic> UpdateOrderStatus(int orderId, string newStatus);
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
        public async Task<dynamic> GetOrdersByUserId(int userId, string? orderStatus)
        {
            var query = _context.Orders
                .Where(o => o.UserId == userId);

            if (!string.IsNullOrWhiteSpace(orderStatus))
            {
                query = query.Where(o => o.OrderStatus == orderStatus);
            }

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.OrderId,
                    o.CreatedAt,
                    o.DeliveryAddress,
                    o.Note,
                    o.OrderStatus,
                    o.TotalAmount,
                    o.PaymentStatus,
                    OrderItems = o.OrderItems.Select(oi => new
                    {
                        oi.FoodId,
                        FoodName = oi.Food!.Name,
                        FoodImageUrl = oi.Food!.ImageUrl,
                        oi.Price,
                        oi.Quantity,
                        oi.Note,
                    }),
                    Payments = o.Payments.Select(p => new
                    {
                        MethodName = p.Method.Name,
                        p.PaymentStatus,
                        p.Amount,
                    })
                })
                .ToListAsync();

            return orders;
        }

        public async Task<dynamic> GetOrderDetails(int userId, int orderId)
        {
            var order = await _context.Orders
                        .Where(o => o.UserId == userId && o.OrderId == orderId)
                        .Select(o => new
                        {
                            o.OrderId,
                            o.CreatedAt,
                            o.DeliveryAddress,
                            o.Note,
                            o.TotalAmount,
                            o.OrderStatus,        
                            o.PaymentStatus,
                            OrderItems = o.OrderItems.Select(oi => new
                            {
                                oi.OrderItemId,
                                oi.Quantity,
                                oi.Price,
                                oi.Note,
                                oi.Food.FoodId,
                                FoodName = oi.Food!.Name,
                                FoodImageUrl = oi.Food!.ImageUrl,
                            }),
                            Payments = o.Payments.Select(p => new
                            {
                                p.PaymentId,
                                p.Amount,
                                p.PaymentStatus,
                                MethodName = p.Method.Name
                            })
                        })
                        .FirstOrDefaultAsync();

            return order;
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

                        var oi = _mapper.Map<OrderItem>(ci);
                        oi.Price = ci.Food.Price;

                        orderItems.Add(oi);
                    }
                    _context.CartItems.RemoveRange(cart.CartItems);
                }
                else
                {
                    // đặt món trực tiếp
                    var foodIds = request.OrderItems.Select(o => o.FoodId).ToList();
                    var foods = await _context.Foods.Where(f => foodIds.Contains(f.FoodId)).ToListAsync();

                    foreach (var item in request.OrderItems)
                    {
                        var food = foods.First(f => f.FoodId == item.FoodId);
                        totalAmount += food.Price * item.Quantity;

                        var oi = _mapper.Map<OrderItem>(item);
                        oi.Price = food.Price;

                        orderItems.Add(oi);
                    }
                }

                var order = _mapper.Map<Order>(request);
                order.UserId = userId;
                order.TotalAmount = totalAmount;
                order.OrderItems = orderItems;

                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new
                {
                    StatusCode = 200,
                    Message = "Đặt hàng thành công",
                    OrderId = order.OrderId,
                    Order = _mapper.Map<OrderMap>(order),
                    Amount = totalAmount,
                };

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new
                {
                    StatusCode = 500,
                    Message = "Lỗi server: " + ex.Message,
                    InnerError = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace
                };
            }
        }

        //public async Task<dynamic> ConfirmReceived(int userId, int orderId)
        //{
        //    var order = await _context.Orders
        //        .Include(o => o.Payments).ThenInclude(p => p.Method)
        //        .FirstOrDefaultAsync(o => o.UserId == userId && o.OrderId == orderId);

        //    order.OrderStatus = "Completed";
        //    order.UpdatedAt = DateTime.Now;

        //    await _context.SaveChangesAsync();
        //    return new
        //    {
        //        StatusCode = 200,
        //        Message = "Xác nhận đã hủy thành công",
        //        OrderId = order.OrderId,
        //        OrderStatus = order.OrderStatus, 
        //        PaymentStatus = order.PaymentStatus
        //    };
        //}

        public async Task<dynamic> ConfirmReceived(int userId, int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Payments)          
                    .ThenInclude(p => p.Method) 
                .FirstOrDefaultAsync(o => o.UserId == userId && o.OrderId == orderId);

            if (order == null)
                return new { StatusCode = 404, Message = "Không tìm thấy đơn hàng" };

            order.OrderStatus = "Completed";
            order.UpdatedAt = DateTime.Now;

            var codPayment = order.Payments
                .FirstOrDefault(p => p.Method.Name == "COD");

            if (codPayment != null && codPayment.PaymentStatus == "Pending")
            {
                order.PaymentStatus = "Success";
                codPayment.PaymentStatus = "Success";
            }

            await _context.SaveChangesAsync();

            return new
            {
                StatusCode = 200,
                Message = "Xác nhận đã nhận hàng thành công",
                OrderId = order.OrderId,
                OrderStatus = order.OrderStatus,
                PaymentStatus = order.PaymentStatus
            };
        }
        public async Task<dynamic> CancelOrder(int userId, int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.Payments).ThenInclude(p => p.Method)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.OrderId == orderId);

            order.OrderStatus = "Cancelled";

            if (order.Payments.Any(p => p.Method.Name == "COD"))
            {
                order.PaymentStatus = "Paid";
                foreach (var p in order.Payments)
                {
                    if (p.Method.Name == "COD") p.PaymentStatus = "Success";
                }
            }

            order.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return new
            {
                StatusCode = 200,
                Message = "Xác nhận đã nhận hàng thành công",
                OrderId = order.OrderId,
                OrderStatus = order.OrderStatus,
                PaymentStatus = order.PaymentStatus
            };
        }
        // Admin
        public async Task<dynamic> GetAllOrders(string? status)
        {
            var orders = await _context.Orders
                .AsQueryable()
                .Where(o => string.IsNullOrEmpty(status) || o.OrderStatus == status)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => new
                {
                    o.UserId,
                    FullName = o.User.FullName,
                    Phone = o.User.Phone,
                    o.OrderId,
                    o.CreatedAt,
                    o.TotalAmount,
                    o.OrderStatus,
                    o.PaymentStatus,
                    o.DeliveryAddress,
                    o.Note,
                    OrderItems = o.OrderItems.Select(oi => new
                    {
                        oi.FoodId,
                        FoodName = oi.Food!.Name,
                        FoodImageUrl = oi.Food!.ImageUrl,
                        oi.Price,
                        oi.Quantity,
                        oi.Note,
                    }),
                    Payments = o.Payments.Select(p => new
                    {
                        MethodName = p.Method.Name,
                        p.PaymentStatus,
                        p.Amount,

                    })
                })
                .ToListAsync();


            return orders;
            
        }
        public async Task<dynamic> GetOrderById(int orderId)
        {
            var orders = await _context.Orders
                 .Where(o => o.OrderId == orderId)
                 .OrderByDescending(o => o.CreatedAt)
                 .Select(o => new
                 {
                     o.UserId,
                     FullName = o.User.FullName,
                     Phone = o.User.Phone,
                     o.OrderId,
                     o.CreatedAt,
                     o.TotalAmount,
                     o.OrderStatus,
                     o.PaymentStatus,
                     o.DeliveryAddress,
                     o.Note,
                     OrderItems = o.OrderItems.Select(oi => new
                     {
                         oi.FoodId,
                         FoodName = oi.Food!.Name,
                         FoodImageUrl = oi.Food!.ImageUrl,
                         oi.Price,
                         oi.Quantity,
                         oi.Note,
                     }),
                     Payments = o.Payments.Select(p => new
                     {
                         MethodName = p.Method.Name,
                         p.PaymentStatus,
                         p.Amount,

                     })

                 })
                 .ToListAsync();


            return orders;
        }
        public async Task<dynamic> UpdateOrderStatus(int orderId, string newStatus)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            
            order.OrderStatus = newStatus;
            order.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return new
            {
                StatusCode = 200,
                Message = "Cập nhật trạng thái đơn hàng thành công",
                UserId = order.UserId,
                OrderId = order.OrderId,
                OrderStatus = order.OrderStatus
            };
        }
    }
}
