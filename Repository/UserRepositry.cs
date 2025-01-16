using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication3.Interface;
using WebApplication3.Maping;
using WebApplication3.Models;

namespace WebApplication3.Repository
{
    public class UserRepositry : IUserRepositry
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserRepositry> _logger; // Added logger

        public UserRepositry(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration, ApplicationDbContext context, ILogger<UserRepositry> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }

        /// Authenticates a user based on email and password.
        public async Task<bool> AuthenticateAsync(string email, string password)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    return await _userManager.CheckPasswordAsync(user, password);
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error authenticating user with email {email}.");
                return false;
            }
        }

        /// Retrieves a user by their email
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                return await _userManager.FindByEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user by email {email}.");
                return null;
            }
        }
        /// Retrieves a user by their ID
        public async Task<User> GetUserByIdAsync(string userId)
        {
            try
            {
                return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user by ID {userId}.");
                return null;
            }
        }

        /// Logs out the current user
        public async Task LogoutUserAsync()
        {
            try
            {
                await _signInManager.SignOutAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging out user.");
            }
        }

        //Registers a new user
        public async Task<bool> RegisterUserAsync(User user, string password)
        {
            try
            {
                var result = await _userManager.CreateAsync(user, password);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error registering user with email {user.Email}.");
                return false;
            }

        }
    }
}
