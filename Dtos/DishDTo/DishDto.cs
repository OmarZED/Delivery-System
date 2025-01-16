using WebApplication3.Models.Enum;

namespace WebApplication3.Dtos.DishDTo
{
    public class DishDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public double Price { get; set; }
        public string Image { get; set; }
        public double Rating { get; set; }
        public bool Vegetarian { get; set; }
        public Category Category { get; set; }
    }
}
