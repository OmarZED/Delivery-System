using AutoMapper;
using WebApplication3.Dtos.DishDTo;
using WebApplication3.Models;

namespace WebApplication3.Maping
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Dish, DishDto>();

        }
    }
}
