using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication3.Dtos.BasketDto;
using WebApplication3.Interface;

namespace WebApplication3.Controllers
{

    [Authorize]
    [ApiController]
    [Route("api/basket")]
    public class BasketController : ControllerBase
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<BasketController> _logger;

        public BasketController(IBasketRepository basketRepository, IMapper mapper, ILogger<BasketController> logger)
        {
            _basketRepository = basketRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// Retrieves the basket of the authenticated user
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(BasketDTO))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetBasket()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Invalid user ID when getting basket.");
                    return BadRequest("Invalid user ID.");
                }

                var basket = await _basketRepository.GetBasketAsync(userId);
                if (basket == null)
                {
                    _logger.LogWarning($"Basket not found for user ID {userId} when getting basket.");
                    return NotFound("Basket not found.");
                }
                return Ok(basket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting basket.");
                return StatusCode(500, "Internal server error");
            }

        }

        /// Adds a dish to the authenticated user's basket
        [HttpPost("dish/{dishId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AddDishToBasket(Guid dishId, [FromQuery] int amount) // Modified AddItem
        {
            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized user attempting to add dish to basket.");
                return BadRequest("User is not authenticated.");
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in the token when adding dish to basket.");
                    return BadRequest("User ID not found in the token.");
                }

                if (amount <= 0)
                {
                    _logger.LogWarning($"Invalid amount {amount} provided when adding dish with ID {dishId} to basket.");
                    return BadRequest("Amount must be greater than 0.");
                }

                var basket = await _basketRepository.AddToBasketAsync(dishId, amount, userId);
                if (basket == null)
                {
                    _logger.LogWarning($"Dish with ID {dishId} not found when adding to basket.");
                    return NotFound("Dish not found.");
                }
                return Ok(new { message = "Dish added to basket" });
            }
            catch (ArgumentException)
            {
                _logger.LogWarning($"Dish with ID {dishId} not found.");
                return BadRequest("Dish not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding dish with ID {dishId} to basket.");
                return BadRequest($"Error: {ex.Message}");
            }
        }
    }
}
