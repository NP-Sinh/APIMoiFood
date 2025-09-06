using APIMoiFood.Models.DTOs.Cart;
using APIMoiFood.Models.Entities;
using APIMoiFood.Models.Mapping;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace APIMoiFood.Services.CartService
{
    public interface ICartService
    {
        // user
        Task<dynamic> GetCart(int userId);
        Task<dynamic> AddToCart(int userId, CartItemRequest request);
        Task<dynamic> UpdateQuantity(int userId, CartItemRequest request);
        Task<dynamic> RemoveFromCart(int userId, int foodId);

        // admin
        // Task<dynamic> GetAllCarts();
        // Task<dynamic> GetCartByUserId(int userId);
    }
    public class CartService : ICartService
    {
        private readonly IMapper _mapper;
        private readonly MoiFoodDBContext _context;
        public CartService(MoiFoodDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<dynamic> GetCart(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Food)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            var cartMap = _mapper.Map<CartMap>(cart);
            return new
            {
                StatusCode = 200,
                Message = "Thành công",
                DataCart = cartMap,
                TotalItems = cartMap.TotalPrice
            };
        }
        public  async Task<dynamic> AddToCart(int userId, CartItemRequest request)
        {
            try
            {
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Food)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if(cart == null)
                {
                    cart = new Cart
                    {
                        UserId = userId,
                        CreatedAt = DateTime.Now,
                        CartItems = new List<CartItem>()
                    };

                    _context.Carts.Add(cart);
                }

                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.FoodId == request.FoodId);
                
                if(cartItem != null)
                {
                    cartItem.Quantity += request.Quantity;
                }
                else
                {
                    var newItem = _mapper.Map<CartItem>(request);
                    newItem.Cart = cart;
                    cart.CartItems.Add(newItem);
                }

                await _context.SaveChangesAsync();
                return new
                {
                    StatusCode = 200,
                    Message = "Thêm vào giỏ hàng thành công",
                    Data = _mapper.Map<CartMap>(cart),

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

        public async Task<dynamic> UpdateQuantity(int userId, CartItemRequest request)
        {
            try
            {
                var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Food)
                .FirstOrDefaultAsync(c => c.UserId == userId);

                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.FoodId == request.FoodId);

                if (request.Quantity <= 0)
                {
                    _context.CartItems.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = request.Quantity;
                }

                await _context.SaveChangesAsync();
                return new
                {
                    StatusCode = 200,
                    Message = "Cập nhật số lượng thành công",
                    Data = _mapper.Map<CartMap>(cart),
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

        public async Task<dynamic> RemoveFromCart(int userId, int foodId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Food)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.FoodId == foodId);

            _context.CartItems.Remove(cartItem);

            await _context.SaveChangesAsync();
            return new
            {
                StatusCode = 200,
                Message = "Xoá khỏi giỏ hàng thành công",
                Data = _mapper.Map<CartMap>(cart),
            };
        }
    }
}
