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

        /// <summary>
        /// Retrieves the basket of the authenticated user.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(BasketDTO))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetBasket()
        {
            var userId = GetUserId();
            if (userId == null)
                return BadRequest("Invalid user ID.");

            try
            {
                var basket = await _basketRepository.GetBasketAsync(userId);
                if (basket == null)
                {
                    _logger.LogWarning("Basket not found for user ID {UserId}", userId);
                    return NotFound("Basket not found.");
                }
                return Ok(basket);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error getting basket.");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Adds a dish to the authenticated user's basket.
        /// </summary>
        [HttpPost("dish/{dishId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AddDishToBasket(Guid dishId, [FromQuery] int amount)
        {
            var userId = GetUserId();
            if (userId == null)
                return BadRequest("User is not authenticated.");

            if (amount <= 0)
            {
                _logger.LogWarning("Invalid amount {Amount} for user ID {UserId}", amount, userId);
                return BadRequest("Amount must be greater than 0.");
            }

            try
            {
                var basket = await _basketRepository.AddToBasketAsync(dishId, amount, userId);
                if (basket == null)
                {
                    _logger.LogWarning("Dish ID {DishId} not found for user ID {UserId}", dishId, userId);
                    return NotFound("Dish not found.");
                }
                return Ok(new { message = "Dish added to basket" });
            }
            catch (ArgumentException)
            {
                return BadRequest("Dish not found.");
            }
            catch (Exception ex)
            {
                LogError(ex, "Error adding dish to basket.");
                return BadRequest(new { message = ex.Message });
            }
        }
        /// <summary>
        /// Removes a specific item from the user's basket. If increase=true, decreases quantity; else removes item.
        /// </summary>
        [HttpDelete("dish/{dishId}")]
        [ProducesResponseType(200, Type = typeof(BasketDTO))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RemoveItem(Guid dishId, [FromQuery] bool increase = false)
        {
            var userId = GetUserId();
            if (userId == null)
                return BadRequest(new { message = "Invalid user ID." });

            try
            {
                var basket = await _basketRepository.RemoveFromBasketAsync(dishId, userId, increase);
                if (basket == null)
                {
                    _logger.LogWarning("Basket item with ID {DishId} not found for user ID {UserId}", dishId, userId);
                    return NotFound(new { message = "Basket item not found" });
                }

                return Ok(basket);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error removing item from basket.");
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }

        private string? GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in token.");
                return null;
            }
            return userId;
        }

        private void LogError(Exception ex, string message)
        {
            var userId = GetUserId();
            _logger.LogError(ex, "{Message} | User ID: {UserId}", message, userId ?? "unknown");
        }
    }
}



