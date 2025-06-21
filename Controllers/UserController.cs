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
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserRepository userRepository, UserManager<User> userManager, ILogger<UserController> logger)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
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
                    _logger.LogWarning($"Email already taken: {registerDto.EmailAddress}");
                    return BadRequest(new { Message = "Email address is already taken." });
                }

                if (!IsPasswordStrong(registerDto.Password))
                {
                    return BadRequest(new { Message = "Password is too weak. Use at least 8 characters, with letters and numbers." });
                }

                var user = new User
                {
                    UserName = registerDto.EmailAddress,
                    Email = registerDto.EmailAddress,
                    Name = registerDto.Name,
                    Address = registerDto.Address,
                    PhoneNumber = registerDto.PhoneNumber.ToString(),
                    BirthDate = registerDto.BirthDate
                };

                var isRegistered = await _userRepository.RegisterUserAsync(user, registerDto.Password);

                return isRegistered
                    ? Ok(new { Message = "User registered successfully!" })
                    : BadRequest(new { Message = "Registration failed." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration.");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Logs in a user and returns a JWT token.
        /// </summary>
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
                    return BadRequest(new { Message = "Invalid login data." });
                }

                var isAuthenticated = await _userRepository.AuthenticateAsync(loginDto.Email, loginDto.Password);

                if (!isAuthenticated)
                {
                    return Unauthorized(new { message = "Invalid email or password." });
                }

                var user = await _userRepository.GetUserByEmailAsync(loginDto.Email);
                var token = await _userRepository.GenerateJwtToken(user);

                return Ok(new { message = "Login successful!", token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed.");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Logs out the authenticated user.
        /// </summary>
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

        /// <summary>
        /// Returns the profile info of the currently authenticated user.
        /// </summary>
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
                    return BadRequest(new { Message = "Invalid or missing user ID." });
                }

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
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
                _logger.LogError(ex, "Error retrieving user profile.");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Updates the profile info of the currently authenticated user.
        /// </summary>
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
                    return BadRequest(ModelState);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userRepository.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(new { Message = "User not found." });
                }

                user.Name = updateProfileDto.Name;
                user.BirthDate = updateProfileDto.BirthDate;
                user.Address = updateProfileDto.Address;
                user.PhoneNumber = updateProfileDto.PhoneNumber.ToString();
                user.Gender = updateProfileDto.Gender;

                var isUpdated = await _userRepository.UpdateUserAsync(user);

                return isUpdated
                    ? Ok(new { Message = "Profile updated successfully!" })
                    : BadRequest(new { Message = "Failed to update profile." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile.");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Validates the strength of a password.
        /// </summary>
        private bool IsPasswordStrong(string password)
        {
            return password.Length >= 8 && password.Any(char.IsLetter) && password.Any(char.IsDigit);
        }
    }
}

