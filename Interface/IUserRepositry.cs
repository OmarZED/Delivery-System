using WebApplication3.Models;

namespace WebApplication3.Interface
{
    public interface IUserRepositry
    {
        Task<bool> RegisterUserAsync(User user, string password);

        Task<bool> AuthenticateAsync(string email, string password);
        Task LogoutUserAsync();
        Task<string> GenerateJwtToken(User user);
        Task<User?> GetUserByEmailAsync(string email);

        Task<User> GetUserByIdAsync(string userId);

        Task<bool> UpdateUserAsync(User user);
    }
}
