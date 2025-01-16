using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
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
        /// Updates an existing user
        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user with ID {user.Id}.");
                return false;
            }
        }

        /// Generates a JWT token for a user
        public async Task<string> GenerateJwtToken(User user)
        {
            try
            {
                // Define claims based on user information
                var claims = new[]
                   {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),  // User ID
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),       // User email
                    new Claim(ClaimTypes.Name, user.UserName),                  // Username
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),   // User ID again for consistency
                    new Claim("Name", user.Name),                               // Custom claim for Name
                    new Claim("BirthDate", user.BirthDate.ToString("yyyy-MM-dd")), // Standardize date format
                    new Claim("Address", user.Address),                         // Address
                    new Claim("Gender", user.Gender.ToString())                 // Gender
                 };

                // Retrieve the signing key from configuration
                var signingKey = _configuration["JWT:SigningKey"];
                if (string.IsNullOrEmpty(signingKey))
                {
                    throw new InvalidOperationException("JWT SigningKey is missing in the configuration.");
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // Create the JWT token
                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:Issuer"],       // Issuer from configuration
                    audience: _configuration["JWT:Audience"],  // Audience from configuration
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(60),   // Token expiration time
                    signingCredentials: creds                  // Signing credentials
                );

                // Return the serialized token
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating JWT token for user {user.Email}.");
                return null; // Or handle the exception as appropriate for your application
            }

        }
    }
}
