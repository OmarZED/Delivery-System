namespace WebApplication3.Dtos.Rating_Dto
{
    public class DishRatingDTO
    {
        public Guid DishId { get; set; }
        public double AverageRating { get; set; }
        public int NumberOfRatings { get; set; }
    }
}
