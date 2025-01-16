using Microsoft.EntityFrameworkCore;
using WebApplication3.Dtos.BasketDto;
using WebApplication3.Interface;
using WebApplication3.Maping;
using WebApplication3.Models;

namespace WebApplication3.Repository
{

    public class BasketRepository : IBasketRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BasketRepository> _logger;

        public BasketRepository(ApplicationDbContext context, ILogger<BasketRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// Adds a dish to the user's basket or updates the quantity if the dish is already present
        public async Task<BasketDTO?> AddToBasketAsync(Guid dishId, int amount, string userId)
        {
            try
            {
                var basket = await GetOrCreateBasketForUserAsync(userId);
                var dish = await _context.Dishes.FindAsync(dishId);

                if (dish == null)
                {
                    _logger.LogWarning($"Dish with ID {dishId} not found when adding to basket.");
                    return null;
                }
                var existingItem = basket.BasketItems.FirstOrDefault(item => item.DishId == dishId);

                if (existingItem != null)
                {
                    existingItem.Amount += amount;
                }
                else
                {
                    var newItem = new BasketItem
                    {

                        BasketId = basket.Id,
                        DishId = dishId,
                        Amount = amount,
                        Name = dish.Name,
                        Price = dish.Price,
                        Image = dish.Image
                    };
                    _context.BasketItems.Add(newItem);
                }

                await _context.SaveChangesAsync();
                return await GetBasketAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding dish with ID {dishId} to basket for user with ID {userId}.");
                return null;
            }

        }

        /// Retrieves the basket of a user by its ID
        public async Task<BasketDTO?> GetBasketAsync(string userId)
        {
            try
            {
                var basket = await _context.Baskets
                .Include(b => b.BasketItems)
                .ThenInclude(bi => bi.Dish)
                .FirstOrDefaultAsync(b => b.UserId == userId);

                if (basket == null)
                {
                    _logger.LogWarning($"Basket not found for user ID {userId}.");
                    return null;
                }
                var basketDto = new BasketDTO
                {
                    Id = basket.Id,
                    UserId = basket.UserId,
                    BasketItems = basket.BasketItems.Select(x => new BasketItemDTO
                    {
                        Id = x.Id,
                        DishId = x.DishId,
                        Name = x.Name,
                        Price = x.Price,
                        Image = x.Image,
                        Amount = x.Amount
                    }).ToList()
                };
                return basketDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting basket for user with ID {userId}.");
                return null;
            }

        }
        /// Removes a specific dish item from the user's basket
        public async Task<BasketDTO?> RemoveFromBasketAsync(Guid dishId, string userId, bool increase)
        {
            try
            {
                var basket = await GetUserBasketAsync(userId);
                if (basket == null)
                {
                    _logger.LogWarning($"Basket not found for user ID {userId} when removing item with ID {dishId}.");
                    return null;
                }

                var itemToRemove = basket.BasketItems.FirstOrDefault(item => item.DishId == dishId);

                if (itemToRemove != null)
                {
                    if (increase)
                    {
                        itemToRemove.Amount--;
                        if (itemToRemove.Amount <= 0)
                        {
                            _context.BasketItems.Remove(itemToRemove);
                        }

                    }
                    else
                    {
                        _context.BasketItems.Remove(itemToRemove);
                    }
                    await _context.SaveChangesAsync();
                }
                return await GetBasketAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing item with ID {dishId} from basket for user with ID {userId}.");
                return null;
            }
        }

        /// Clears all items from the user's basket
        public async Task<bool> ClearBasketAsync(string userId)
        {
            try
            {
                var basket = await GetUserBasketAsync(userId);
                if (basket != null)
                {
                    _context.BasketItems.RemoveRange(basket.BasketItems);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error clearing basket for user with ID {userId}.");
                return false;
            }
        }

    }
}
