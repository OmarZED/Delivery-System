using WebApplication3.Models;

namespace WebApplication3.Interface
{
    public interface IUserRepository
    {
        // === Authentication ===

        /// <summary>
        /// Registers a new user with the specified password.
        /// </summary>
        Task<bool> RegisterUserAsync(User user, string password);

        /// <summary>
        /// Authenticates the user with the provided email and password.
        /// </summary>
        Task<bool> AuthenticateAsync(string email, string password);

        /// <summary>
        /// Logs out the current user.
        /// </summary>
        Task LogoutUserAsync();

        // === Token ===

        /// <summary>
        /// Generates a JWT token for the specified user.
        /// </summary>
        Task<string> GenerateJwtToken(User user);

        // === Retrieval ===

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        Task<User?> GetUserByEmailAsync(string email);

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        Task<User> GetUserByIdAsync(string userId);

        // === Update ===

        /// <summary>
        /// Updates the user profile.
        /// </summary>
        Task<bool> UpdateUserAsync(User user);
    }
}