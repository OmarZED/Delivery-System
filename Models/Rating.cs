namespace WebApplication3.Models
{
    public class Rating
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public Guid DishId { get; set; }
        public Dish Dish { get; set; }
        public int Score { get; set; } // Rating score (e.g., 1 to 5)
        public DateTime CreatedAt { get; set; }
    }
}
