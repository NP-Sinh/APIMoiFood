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

            var itemsWithTotal = cart.CartItems.Select(ci => new
            {
                ci.CartItemId,
                ci.FoodId,
                ci.Quantity,
                Price = ci.Food?.Price ?? 0,
                ItemTotal = (ci.Food?.Price ?? 0) * ci.Quantity
            }).ToList();

            decimal totalPrice = itemsWithTotal.Sum(i => i.ItemTotal);

            return new
            {
                StatusCode = 200,
                Message = "Thành công",
                DataCart = new
                {
                    cart.CartId,
                    cart.UserId,
                    Items = itemsWithTotal
                },
                TotalPrice = totalPrice
            };
        }

        public async Task<dynamic> AddToCart(int userId, CartItemRequest request)
        {
            try
            {
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Food)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if(cart == null)
                {
                    var user = new User { UserId = userId };
                    cart = _mapper.Map<Cart>(user);

                    _context.Carts.Add(cart);
                }

                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.FoodId == request.FoodId);

                var food = await _context.Foods.FindAsync(request.FoodId);

                if (cartItem != null)
                {
                    cartItem.Quantity += request.Quantity;
                }
                else
                {
                    var newItem = _mapper.Map<CartItem>(request);

                    newItem.Cart = cart;
                    newItem.Food = food;
                    cart.CartItems.Add(newItem);
                }

                decimal totalPrice = cart.CartItems.Sum(ci => (ci.Quantity * (ci.Food?.Price ?? 0)));
                await _context.SaveChangesAsync();
                return new
                {
                    StatusCode = 200,
                    Message = "Thêm vào giỏ hàng thành công",
                    Data = _mapper.Map<CartMap>(cart),
                    TotalPrice = totalPrice

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
                decimal totalPrice = cart.CartItems.Sum(ci => (ci.Quantity * (ci.Food?.Price ?? 0)));
                await _context.SaveChangesAsync();
                return new
                {
                    StatusCode = 200,
                    Message = "Cập nhật số lượng thành công",
                    Data = _mapper.Map<CartMap>(cart),
                    TotalPrice = totalPrice
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

            decimal totalPrice = cart.CartItems
               .Where(ci => ci.FoodId != foodId)
               .Sum(ci => ci.Quantity * (ci.Food?.Price ?? 0));

            return new
            {
                StatusCode = 200,
                Message = "Xoá khỏi giỏ hàng thành công",
                Data = _mapper.Map<CartMap>(cart),
                TotalPrice = totalPrice,
            };
        }
    }
}
