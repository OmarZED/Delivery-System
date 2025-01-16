using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication3.Dtos.UserDto;
using WebApplication3.Interface;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepositry _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepositry userRepository, UserManager<User> userManager, ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _logger = logger;
        }

        /// Registers a new user.
        [HttpPost("Register")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state in register request.");
                    return BadRequest(ModelState);
                }

                var existingUser = await _userRepository.GetUserByEmailAsync(registerDto.EmailAddress);
                if (existingUser != null)
                {
                    _logger.LogWarning($"Attempt to register user with already registered email {registerDto.EmailAddress}.");
                    return BadRequest(new { Message = "Email address is already taken." });
                }

                if (!IsPasswordStrong(registerDto.Password))
                {
                    _logger.LogWarning("Password is too weak.");
                    return BadRequest(new { Message = "Password is too weak. Please ensure it contains a mix of letters, numbers, and special characters." });
                }

                var user = new User
                {
                    UserName = registerDto.EmailAddress, // Required for UserManager
                    Email = registerDto.EmailAddress,
                    Name = registerDto.Name,
                    Address = registerDto.Address,
                    PhoneNumber = registerDto.PhoneNumber.ToString(),
                    BirthDate = registerDto.BirthDate,

                };

                var isRegistered = await _userRepository.RegisterUserAsync(user, registerDto.Password);

                if (isRegistered)
                    return Ok(new { Message = "User registered successfully!" });

                _logger.LogError($"Registration failed for user with email {user.Email}.");
                return BadRequest(new { Message = "Registration failed." });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Register");
                return StatusCode(500, "Internal server error");
            }

        }
        /// Logs in a user and generates a JWT token.
        [HttpPost("Login")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state in login request.");
                    return BadRequest(new { Message = "Invalid data.", Errors = ModelState.Values.SelectMany(v => v.Errors) });
                }


                var isAuthenticated = await _userRepository.AuthenticateAsync(loginDto.Email, loginDto.Password);

                if (!isAuthenticated)
                {
                    _logger.LogWarning($"Invalid email or password provided for user with email {loginDto.Email}.");
                    return Unauthorized(new { message = "Invalid email or password." });
                }

                var user = await _userRepository.GetUserByEmailAsync(loginDto.Email);
                var token = await _userRepository.GenerateJwtToken(user);

                return Ok(new { message = "Login successful!", token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error logging in user with email {loginDto.Email}.");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpPost("Logout")]
        [Authorize]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _userRepository.LogoutUserAsync();
                return Ok(new { message = "User logged out successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging out user.");
                return StatusCode(500, "Internal server error");
            }

        }
        /// Gets the profile information of the current user.
        [Authorize]
        [HttpGet("Profile")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Invalid or missing token provided when getting profile.");
                    return BadRequest(new { Message = "Invalid or missing token." });
                }

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning($"User not found for ID {userId}.");
                    return NotFound(new { Message = "User not found." });
                }

                var userDto = new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    user.BirthDate,
                    user.Address,
                    user.PhoneNumber
                };
                return Ok(userDto);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile.");
                return StatusCode(500, "Internal server error");
            }
        }
        /// Updates the profile information of the current user
        [Authorize]
        [HttpPut("UpdateProfile")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid model state in update profile request.");
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Adjust as per your claims
                var user = await _userRepository.GetUserByIdAsync(userId);

                if (user == null)
                {
                    _logger.LogWarning($"User not found with ID {userId} when updating profile.");
                    return NotFound(new { Message = "User not found" });
                }

                // Update fields
                user.Name = updateProfileDto.Name;
                user.BirthDate = updateProfileDto.BirthDate;
                user.Address = updateProfileDto.Address;
                user.PhoneNumber = updateProfileDto.PhoneNumber.ToString();
                user.Gender = updateProfileDto.Gender;

                // Save changes
                var isUpdated = await _userRepository.UpdateUserAsync(user);
                if (!isUpdated)
                {
                    _logger.LogError($"Failed to update profile for user with ID {userId}");
                    return BadRequest(new { Message = "Failed to update profile" });
                }

                return Ok(new { Message = "Profile updated successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile.");
                return StatusCode(500, "Internal server error");
            }

        }

        /// Checks if the password meet the complexity standards.
        private bool IsPasswordStrong(string password)
        {
            // Example: Password should be at least 8 characters long and contain a mix of letters and numbers.
            return password.Length >= 8 && password.Any(char.IsLetter) && password.Any(char.IsDigit);
        }
    }
}
   
    
