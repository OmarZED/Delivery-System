namespace WebApplication3.Models
{
    public class BasketItem
    {
        public Guid Id { get; set; }
        public Guid BasketId { get; set; }
        public Basket Basket { get; set; }
        public Guid DishId { get; set; }
        public Dish Dish { get; set; }
        public int Amount { get; set; }

        public string Name { get; set; }
        public double Price { get; set; }
        public string Image { get; set; }
    }
}
