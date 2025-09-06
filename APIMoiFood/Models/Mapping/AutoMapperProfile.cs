using APIMoiFood.Models.DTOs.Cart;
using APIMoiFood.Models.DTOs.Category;
using APIMoiFood.Models.DTOs.Food;
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
             .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.CartItems));

            CreateMap<CartItem, CartItemMap>()
                .ForMember(dest => dest.FoodName, opt => opt.MapFrom(src => src.Food != null ? src.Food.Name : null))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Food != null ? src.Food.Price : 0))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Food != null ? src.Quantity * src.Food.Price : 0));

            CreateMap<CartRequest, Cart>();
            CreateMap<CartItemRequest, CartItem>();

        }

    }
}