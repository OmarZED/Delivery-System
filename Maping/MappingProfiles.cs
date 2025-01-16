using AutoMapper;
using WebApplication3.Dtos.BasketDto;
using WebApplication3.Dtos.DishDTo;
using WebApplication3.Models;

namespace WebApplication3.Maping
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Dish, DishDto>();

            CreateMap<Basket, BasketDTO>()
.ForMember(dest => dest.BasketItems, opt => opt.MapFrom(src => src.BasketItems));
            CreateMap<BasketItem, BasketItemDTO>();
        }
    }
}
