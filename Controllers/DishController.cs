using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication3.Dtos.DishDTo;
using WebApplication3.Interface;
using WebApplication3.Models.Enum;

namespace WebApplication3.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class DishController : ControllerBase
    {
        private readonly IDishRepositry _dishRepositry;
        private readonly IRatingRepository _ratingRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<DishController> _logger;

        public DishController(IDishRepositry dishRepositry, IRatingRepository ratingRepository, IMapper mapper, ILogger<DishController> logger)
        {
            _dishRepositry = dishRepositry;
            _ratingRepository = ratingRepository;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<DishDto>))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetDishes(
      [FromQuery] Category[]? categories, // Use the enum here
        [FromQuery] bool? Vegetarian,
        [FromQuery] DishSorting? sortBy, // Use the enum here
        [FromQuery] int page = 1)
        {
            try
            {
                if (categories != null && categories.Any(c => !Enum.IsDefined(typeof(Category), c)))
                {
                    _logger.LogWarning($"Invalid category value provided. Valid values: {string.Join(", ", Enum.GetNames(typeof(Category)))}");
                    return BadRequest(new { message = $"Invalid category value provided. Valid values: {string.Join(", ", Enum.GetNames(typeof(Category)))}" });
                }

                var dishes = await _dishRepositry.GetDishes(categories, Vegetarian, sortBy, page);

                if (dishes == null)
                {
                    return NotFound();
                }

                // Get the rating for each dish, map it to DTO, and return the result.
                var dishDtos = await Task.WhenAll(dishes.Select(async dish =>
                {
                    var dishDto = _mapper.Map<DishDto>(dish);
                    var ratingDto = await _ratingRepository.GetDishRatingAsync(dish.Id);
                    if (ratingDto != null)
                        dishDto.Rating = ratingDto.AverageRating;

                    return dishDto;
                }));


                return Ok(dishDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDishes");
                return StatusCode(500, new { message = "Internal server error", detail = ex.Message });
            }
        }
        /// Retrieves a single dish by its ID.
        [HttpGet("{id}")]
        [ProducesResponseType(200, Type = typeof(DishDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetDish(Guid id)
        {
            try
            {
                if (!_dishRepositry.DishExists(id))
                {
                    _logger.LogWarning($"Dish with ID {id} not found.");
                    return NotFound();
                }

                var dish = _dishRepositry.GetDish(id);
                var dishDto = _mapper.Map<DishDto>(dish);

                var ratingDto = await _ratingRepository.GetDishRatingAsync(id);
                if (ratingDto != null)
                    dishDto.Rating = ratingDto.AverageRating;


                if (!ModelState.IsValid)
                {
                    _logger.LogWarning($"Invalid model state when getting dish with ID {id}");
                    return BadRequest(ModelState);
                }

                return Ok(dishDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting dish with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }
        /// Retrieves the rating for a specific dish.

        [HttpGet("{dishId}/rating")]
        [Authorize]
        [ProducesResponseType(200, Type = typeof(bool))]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CanRateDish(Guid dishId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized user attempting to check if they can rate a dish.");
                return Unauthorized();
            }

            try
            {
                bool canRate = await _ratingRepository.CanUserRateDishAsync(userId, dishId);
                return Ok(canRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if user can rate dish with ID {dishId}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}



