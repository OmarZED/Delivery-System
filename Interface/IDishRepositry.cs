using WebApplication3.Models.Enum;
using WebApplication3.Models;
using WebApplication3.Dtos.Rating_Dto;

namespace WebApplication3.Interface
{
    public interface IDishRepositry
    {
        Task<ICollection<Dish>> GetDishes(
              Category[]? categories,
               bool? Vegetarian,
            DishSorting? sortBy,
               int page = 1

            );
        Dish GetDish(Guid Id);
        bool DishExists(Guid Id);
      
    }
}
