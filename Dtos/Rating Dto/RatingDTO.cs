namespace WebApplication3.Dtos.Rating_Dto
{
    public class RatingDTO
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public Guid DishId { get; set; }
        public int Score { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
