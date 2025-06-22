using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Dtos.Rating_Dto;
using WebApplication3.Interface;
using WebApplication3.Maping;
using WebApplication3.Models;

namespace WebApplication3.Repository
{
    public class RatingRepository : IRatingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<RatingRepository> _logger;

        public RatingRepository(ApplicationDbContext context, IMapper mapper, ILogger<RatingRepository> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        /// Checks if a user has ordered a particular dish.
        private async Task<bool> UserHasOrderedDishAsync(string userId, Guid dishId)
        {
            try
            {
                return await _context.Orders
                    .Where(o => o.UserId == userId)
                    .AnyAsync(o => o.OrderItems.Any(oi => oi.DishId == dishId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if user {userId} has ordered dish {dishId}.");
                return false;
            }
        }

        public async Task<bool> CanUserRateDishAsync(string userId, Guid dishId)
        {
            var canRate = await UserHasOrderedDishAsync(userId, dishId);
            if (!canRate)
            {
                _logger.LogInformation($"User {userId} cannot rate dish {dishId} because it was not ordered.");
            }
            return canRate;
        }

        /// Creates or updates a rating for a dish by a user.
        public async Task<RatingDTO?> CreateRatingAsync(CreateRatingDTO createRatingDto, string userId, Guid dishId)
        {
            try
            {
                if (!await UserHasOrderedDishAsync(userId, dishId))
                {
                    _logger.LogWarning($"User {userId} attempted to rate dish {dishId} without ordering it.");
                    throw new InvalidOperationException("You have not ordered this dish.");
                }

                if (createRatingDto.Score < 1 || createRatingDto.Score > 5)
                {
                    _logger.LogWarning($"Invalid rating score {createRatingDto.Score} by user {userId} for dish {dishId}.");
                    throw new ArgumentOutOfRangeException(nameof(createRatingDto.Score), "Score must be between 1 and 5.");
                }

                var existingRating = await _context.Ratings
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.DishId == dishId);

                if (existingRating != null)
                {
                    return await UpdateRatingAsync(existingRating, createRatingDto.Score);
                }
                else
                {
                    return await CreateNewRatingAsync(userId, dishId, createRatingDto.Score);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating/updating rating for user {userId} and dish {dishId}.");
                throw;
            }
        }

        private async Task<RatingDTO> UpdateRatingAsync(Rating existingRating, int newScore)
        {
            _logger.LogInformation($"Updating rating {existingRating.Id} for dish {existingRating.DishId} by user {existingRating.UserId}.");
            existingRating.Score = newScore;
            existingRating.CreatedAt = DateTime.UtcNow;
            _context.Ratings.Update(existingRating);
            await _context.SaveChangesAsync();
            return _mapper.Map<RatingDTO>(existingRating);
        }

        private async Task<RatingDTO> CreateNewRatingAsync(string userId, Guid dishId, int score)
        {
            _logger.LogInformation($"Creating new rating for dish {dishId} by user {userId}.");
            var rating = new Rating
            {
                Id = Guid.NewGuid(),
                DishId = dishId,
                UserId = userId,
                Score = score,
                CreatedAt = DateTime.UtcNow
            };
            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();
            return _mapper.Map<RatingDTO>(rating);
        }

        /// Retrieves the average rating for a dish.
        public async Task<DishRatingDTO?> GetDishRatingAsync(Guid dishId)
        {
            try
            {
                var ratings = await _context.Ratings.Where(r => r.DishId == dishId).ToListAsync();

                var ratingDto = new DishRatingDTO
                {
                    DishId = dishId,
                    AverageRating = ratings.Count == 0 ? 0 : ratings.Average(r => r.Score),
                    NumberOfRatings = ratings.Count
                };

                if (ratings.Count == 0)
                    _logger.LogWarning($"No ratings found for dish {dishId}.");

                return ratingDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting dish rating for dish {dishId}.");
                throw;
            }
        }
    }
