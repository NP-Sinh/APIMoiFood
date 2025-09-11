using APIMoiFood.Models.DTOs.Auth;
using APIMoiFood.Models.DTOs.Cart;
using APIMoiFood.Models.DTOs.Category;
using APIMoiFood.Models.DTOs.Food;
using APIMoiFood.Models.DTOs.Order;
using APIMoiFood.Models.DTOs.Payment;
using APIMoiFood.Models.Entities;
using AutoMapper;

namespace APIMoiFood.Models.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserMap>().ReverseMap();

            // RefreshToken <-> RefreshTokenMap
            CreateMap<RefreshToken, RefreshTokenMap>()
                .ForMember(dest => dest.RefreshToken1, opt => opt.MapFrom(src => src.RefreshToken1))
                .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate))
                .ForMember(dest => dest.TokenId, opt => opt.MapFrom(src => src.TokenId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.IsRevoked, opt => opt.MapFrom(src => src.IsRevoked))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ReverseMap();

            // RefreshToken <-> RefreshTokenMap
            CreateMap<RefreshToken, RefreshTokenMap>().ReverseMap();

            // RegisterRequest -> User
            CreateMap<RegisterRequest, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => "User"))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            // Category mapping
            CreateMap<Category, CategoryMap>()
                .ForMember(dest => dest.Foods, opt => opt.MapFrom(src => src.Foods))
                .ReverseMap();

            CreateMap<CategoryRequest, Category>().ReverseMap();

            // Food mapping
            CreateMap<Food, FoodMap>().ReverseMap();

            CreateMap<FoodRequest, Food>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => true)).ReverseMap();

            // Cart mapping
            CreateMap<Cart, CartMap>()
                .ForMember(dest => dest.CartItems, opt => opt.MapFrom(src => src.CartItems))
                .ReverseMap();

            CreateMap<CartItemRequest, CartItem>()
                .ForMember(dest => dest.CartItemId, opt => opt.Ignore())
                .ForMember(dest => dest.Cart, opt => opt.Ignore())
                .ForMember(dest => dest.Food, opt => opt.Ignore());

            // Mapping CartRequest -> Cart
            CreateMap<CartRequest, Cart>()
                .ForMember(dest => dest.CartId, opt => opt.Ignore())
                .ForMember(dest => dest.CartItems, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // CartItem <-> CartItemMap
            CreateMap<CartItem, CartItemMap>().ReverseMap();

            // cartItem -> orderItem
            CreateMap<CartItem, OrderItem>()
                .ForMember(dest => dest.FoodId, opt => opt.MapFrom(src => src.FoodId))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Food.Price))
                .ForMember(dest => dest.Note, opt => opt.Ignore())
                .ReverseMap();

            // OrderItemRequest -> OrderItem
            CreateMap<OrderItemRequest, OrderItem>()
                .ForMember(dest => dest.Price, opt => opt.Ignore())
                .ReverseMap();

            // OrderRequest -> Order
            CreateMap<OrderRequest, Order>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore()) 
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore())
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => "pending"))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => "pending"))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.Now))
                .ReverseMap();

            // Entity -> DTO
            CreateMap<OrderItem, OrderItemMap>()
                .ForMember(dest => dest.FoodName, opt => opt.MapFrom(src => src.Food.Name));

            CreateMap<Order, OrderMap>()
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems))
                .ForMember(dest => dest.Payments, opt => opt.MapFrom(src => src.Payments));

            CreateMap<Payment, PaymentMap>();

            CreateMap<PaymentRequest, Payment>()
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => "pending"))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.Now))
                .ReverseMap();

            CreateMap<PaymentMethod, PaymentMethodMap>();
            CreateMap<PaymentMethodRequest, PaymentMethod>();
        }

    }
}