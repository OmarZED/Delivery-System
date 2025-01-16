using WebApplication3.Dtos.Rating_Dto;

namespace WebApplication3.Repository
{
    public interface IRatingRepository
    {
        Task<DishRatingDTO?> GetDishRatingAsync(Guid dishId);
        Task<RatingDTO?> CreateRatingAsync(CreateRatingDTO createRatingDto, string userId, Guid dishId);
        // New method for checking if a user can rate a dish
        Task<bool> CanUserRateDishAsync(string userId, Guid dishId);
    }
}
