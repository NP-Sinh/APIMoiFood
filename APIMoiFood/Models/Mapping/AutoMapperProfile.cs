using APIMoiFood.Models.DTOs.Cart;
using APIMoiFood.Models.DTOs.Category;
using APIMoiFood.Models.DTOs.Food;
using APIMoiFood.Models.DTOs.Order;
using APIMoiFood.Models.Entities;
using AutoMapper;

namespace APIMoiFood.Models.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Category, CategoryMap>().ReverseMap();
            CreateMap<CategoryRequest, Category>();

            // food mapping with category name
            CreateMap<Food, FoodMap>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
            CreateMap<FoodRequest, Food>();

            // cart mapping
            CreateMap<Cart, CartMap>()
             .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.CartItems)).ReverseMap();

            CreateMap<CartItem, CartItemMap>()
                .ForMember(dest => dest.FoodName, opt => opt.MapFrom(src => src.Food != null ? src.Food.Name : null))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Food != null ? src.Food.Price : 0))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Food != null ? src.Quantity * src.Food.Price : 0));

            CreateMap<CartRequest, Cart>().ReverseMap();
            CreateMap<CartItemRequest, CartItem>().ReverseMap();

            // Order mapping
            CreateMap<Order, OrderMap>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems))
                .ForMember(dest => dest.Payments, opt => opt.MapFrom(src => src.Payments)).ReverseMap();

            CreateMap<OrderItem, OrderItemMap>()
                .ForMember(dest => dest.FoodName, opt => opt.MapFrom(src => src.Food.Name)).ReverseMap();

            CreateMap<Payment, PaymentMap>()
                .ForMember(dest => dest.Method, opt => opt.MapFrom(src => src.Method.Name))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.PaymentStatus)).ReverseMap();
            
            CreateMap<OrderRequest, Order>().ReverseMap();
            CreateMap<OrderItemRequest, OrderItem>().ReverseMap();

        }

    }
}