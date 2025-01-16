using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

                if (!IsPassworgtdStrong(registerDto.Password))
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
    }
}
   
    
