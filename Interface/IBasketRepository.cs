using WebApplication3.Dtos.BasketDto;
using WebApplication3.Models;

namespace WebApplication3.Interface
{
    public interface IBasketRepository
    {
        Task<BasketDTO?> AddToBasketAsync(Guid dishId, int amount, string userId);
        Task<BasketDTO?> GetBasketAsync(string userId);
        Task<BasketDTO?> RemoveFromBasketAsync(Guid dishId, string userId, bool increase);
        Task<bool> ClearBasketAsync(string userId);
        Task<Basket?> GetUserBasketAsync(string userId);
    }
}
