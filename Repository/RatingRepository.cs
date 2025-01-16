using AutoMapper;
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

        public async Task<bool> CanUserRateDishAsync(string userId, Guid dishId)
        {
            try
            {
                // Check if the user has ordered the dish
                bool dishInOrder = await _context.Orders
                    .Where(o => o.UserId == userId)
                    .AnyAsync(o => o.OrderItems.Any(oi => oi.DishId == dishId));

                if (!dishInOrder)
                {
                    _logger.LogInformation($"User with ID {userId} has not ordered dish with ID {dishId} and cannot rate.");
                    return false; // User cannot rate if they haven't ordered
                }

                // If the user has ordered the dish, they are allowed to rate (or update)
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if user {userId} can rate dish {dishId}.");
                return false; // Consider a default of false for exceptions
            }
        }

        /// Creates a new rating for a dish by a user
        public async Task<RatingDTO?> CreateRatingAsync(CreateRatingDTO createRatingDto, string userId, Guid dishId)
        {
            try
            {
                var userOrders = await _context.Orders
                    .Include(o => o.OrderItems)
                    .Where(o => o.UserId == userId)
                    .ToListAsync();

                if (userOrders == null || userOrders.Count == 0)
                {
                    _logger.LogWarning($"No orders found for user with ID {userId} when creating a rating.");
                    throw new Exception("You have not placed any orders");
                }

                var dishInOrder = userOrders.Any(order => order.OrderItems.Any(oi => oi.DishId == dishId));

                if (!dishInOrder)
                {
                    _logger.LogWarning($"User with ID {userId} has not ordered dish with ID {dishId} when creating rating.");
                    throw new Exception("You have not ordered this dish");
                }

                if (createRatingDto.Score < 1 || createRatingDto.Score > 5)
                {
                    _logger.LogWarning($"Invalid score value {createRatingDto.Score} provided when creating a rating for dish with ID {dishId}");
                    throw new Exception("The score must be between 1 and 5.");
                }

                // Check if a rating already exists for the user and dish
                var existingRating = await _context.Ratings
                    .FirstOrDefaultAsync(r => r.UserId == userId && r.DishId == dishId);

                if (existingRating != null)
                {
                    // Update existing rating
                    _logger.LogInformation($"Updating existing rating for dish {dishId} by user {userId}");
                    existingRating.Score = createRatingDto.Score;
                    existingRating.CreatedAt = DateTime.UtcNow; // Optionally update timestamp on update
                    _context.Ratings.Update(existingRating);
                    await _context.SaveChangesAsync();
                    return _mapper.Map<RatingDTO>(existingRating);
                }
                else
                {
                    // Create new rating
                    _logger.LogInformation($"Creating new rating for dish {dishId} by user {userId}");
                    var rating = new Rating
                    {
                        Id = Guid.NewGuid(),
                        DishId = dishId,
                        UserId = userId,
                        Score = createRatingDto.Score,
                        CreatedAt = DateTime.UtcNow,
                    };
                    _context.Ratings.Add(rating);
                    await _context.SaveChangesAsync();
                    return _mapper.Map<RatingDTO>(rating);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating/updating rating for user with ID {userId} and dish ID {dishId}.");
                throw;
            }
        }
    }
}
