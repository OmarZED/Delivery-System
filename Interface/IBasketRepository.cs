using WebApplication3.Dtos.BasketDto;
using WebApplication3.Models;

namespace WebApplication3.Interface
{
    public interface IBasketRepository
    {
      
        /// Adds a dish to the user's basket or increases its quantity if it already exists.
        Task<BasketDTO?> AddToBasketAsync(Guid dishId, int amount, string userId);
        /// Retrieves the current basket of the specified user.
        Task<BasketDTO?> GetBasketAsync(string userId);
        /// Removes a dish completely from the user's basket.
        Task<BasketDTO?> RemoveItemFromBasketAsync(Guid dishId, string userId);

        /// Updates the quantity of a dish in the user's basket by increasing or decreasing.
        Task<BasketDTO?> UpdateBasketItemQuantityAsync(Guid dishId, string userId, bool increase);

        /// Removes all items from the user's basket.
        Task<bool> ClearBasketAsync(string userId);

        /// Retrieves the user's basket as an entity model, not DTO.
        Task<Basket?> GetUserBasketAsync(string userId);
    }
}