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
        /// <summary>
        /// Adds a dish to the user's basket or updates the quantity if the dish already exists.
        /// </summary>
        public async Task<BasketDTO?> AddToBasketAsync(Guid dishId, int amount, string userId)
        {
            try
            {
                var basket = await GetOrCreateBasketForUserAsync(userId);
                var dish = await _context.Dishes.FindAsync(dishId);

                if (dish == null)
                {
                    LogErrorWithContext(null, "Dish not found when adding to basket.", userId, dishId);
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
                LogErrorWithContext(ex, "Error adding dish to basket.", userId, dishId);
                return null;
            }
        }


        /// <summary>
        /// Retrieves the basket of a user by user ID.
        /// </summary>
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
                    LogErrorWithContext(null, "Basket not found.", userId);
                    return null;
                }

                return new BasketDTO
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
            }
            catch (Exception ex)
            {
                LogErrorWithContext(ex, "Error getting basket.", userId);
                return null;
            }
        }

        /// <summary>
        /// Decreases the quantity of a specific dish item in the basket.
        /// </summary>
        public async Task<BasketDTO?> DecreaseItemQuantityAsync(Guid dishId, string userId)
        {
            try
            {
                var basket = await LoadUserBasketWithItemsAsync(userId);
                if (basket == null)
                {
                    LogErrorWithContext(null, "Basket not found when decreasing item quantity.", userId, dishId);
                    return null;
                }

                var item = basket.BasketItems.FirstOrDefault(i => i.DishId == dishId);
                if (item != null)
                {
                    item.Amount--;
                    if (item.Amount <= 0)
                    {
                        _context.BasketItems.Remove(item);
                    }
                    await _context.SaveChangesAsync();
                }

                return await GetBasketAsync(userId);
            }
            catch (Exception ex)
            {
                LogErrorWithContext(ex, "Error decreasing item quantity.", userId, dishId);
                return null;
            }
        }


        /// <summary>
        /// Removes a specific dish item from the user's basket.
        /// </summary>
        public async Task<BasketDTO?> RemoveItemFromBasketAsync(Guid dishId, string userId)
        {
            try
            {
                var basket = await LoadUserBasketWithItemsAsync(userId);
                if (basket == null)
                {
                    LogErrorWithContext(null, "Basket not found when removing item.", userId, dishId);
                    return null;
                }

                var item = basket.BasketItems.FirstOrDefault(i => i.DishId == dishId);
                if (item != null)
                {
                    _context.BasketItems.Remove(item);
                    await _context.SaveChangesAsync();
                }

                return await GetBasketAsync(userId);
            }
            catch (Exception ex)
            {
                LogErrorWithContext(ex, "Error removing item from basket.", userId, dishId);
                return null;
            }
        }
        /// <summary>
        /// Clears all items from the user's basket.
        /// </summary>
        public async Task<bool> ClearBasketAsync(string userId)
        {
            try
            {
                var basket = await LoadUserBasketWithItemsAsync(userId);
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
                LogErrorWithContext(ex, "Error clearing basket.", userId);
                return false;
            }
        }

        private async Task<Basket> GetOrCreateBasketForUserAsync(string userId)
        {
            try
            {
                var basket = await _context.Baskets.FirstOrDefaultAsync(b => b.UserId == userId);
                if (basket == null)
                {
                    basket = new Basket { UserId = userId, Id = Guid.NewGuid() };
                    _context.Baskets.Add(basket);
                    await _context.SaveChangesAsync();
                }
                return basket;
            }
            catch (Exception ex)
            {
                LogErrorWithContext(ex, "Error getting or creating basket.", userId);
                return null;
            }
        }

        private async Task<Basket> LoadUserBasketWithItemsAsync(string userId)
        {
            try
            {
                return await _context.Baskets
                    .Include(b => b.BasketItems)
                    .FirstOrDefaultAsync(b => b.UserId == userId);
            }
            catch (Exception ex)
            {
                LogErrorWithContext(ex, "Error loading basket with items.", userId);
                return null;
            }
        }

        private void LogErrorWithContext(Exception ex, string message, string userId = "", Guid? dishId = null)
        {
            var contextInfo = $"User ID: {userId}, Dish ID: {dishId?.ToString() ?? "N/A"}";
            _logger.LogError(ex, $"{message} | Context: {contextInfo}");
        }

        async Task<Basket?> IBasketRepository.GetUserBasketAsync(string userId)
        {
            return await _context.Baskets.Include(b => b.BasketItems).FirstOrDefaultAsync(b => b.UserId == userId);
        }
    }
}