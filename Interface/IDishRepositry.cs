using WebApplication3.Models.Enum;
using WebApplication3.Models;
using WebApplication3.Dtos.Rating_Dto;

namespace WebApplication3.Interface
{
    public interface IDishRepositry
    {
        Task<ICollection<Dish>> GetDishes(DishQueryParams queryParams);
        Dish GetDish(Guid Id);
        bool DishExists(Guid Id);
      
    }
    public class DishQueryParams
    {
        public Category[]? Categories { get; set; }
        public bool? Vegetarian { get; set; }
        public DishSorting? SortBy { get; set; }
        public int Page { get; set; } = 1;
    }
}
