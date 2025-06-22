using AutoMapper;
using WebApplication3.Dtos.BasketDto;
using WebApplication3.Dtos.DishDTo;
using WebApplication3.Dtos.OrderDto;
using WebApplication3.Dtos.Rating_Dto;
using WebApplication3.Models;

namespace WebApplication3.Maping
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            // Dish entity to DTO
            CreateMap<Dish, DishDto>();

            // Basket and BasketItem to their DTOs
            CreateMap<Basket, BasketDTO>()
                .ForMember(dest => dest.BasketItems, opt => opt.MapFrom(src => src.BasketItems));
            CreateMap<BasketItem, BasketItemDTO>();

            // Order and OrderItem to their DTOs
            CreateMap<Order, OrderDTO>()
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems));
            CreateMap<OrderItem, OrderItemDTO>();
            CreateMap<Order, OrderInfoDTO>();

            // Rating entity to DTO
            CreateMap<Rating, RatingDTO>();
        }
    }
}
