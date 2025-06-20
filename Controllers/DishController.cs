using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication3.Dtos.DishDTo;
using WebApplication3.Dtos.Rating_Dto;
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
            _dishRepositry = dishRepositry ?? throw new ArgumentNullException(nameof(dishRepositry));
            _ratingRepository = ratingRepository ?? throw new ArgumentNullException(nameof(ratingRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DishDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DishDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetDishes([FromQuery] Category[]? categories, [FromQuery] bool? vegetarian, [FromQuery] DishSorting? sortBy, [FromQuery] int page = 1)
        {
            try
            {
                if (categories != null && categories.Any(c => !Enum.IsDefined(typeof(Category), c)))
                {
                    return BadRequest(new { message = $"Invalid category value provided. Valid values: {string.Join(", ", Enum.GetNames(typeof(Category)))}" });
                }

                var dishes = await _dishRepositry.GetDishes(categories, vegetarian, sortBy, page);
                var dishDtos = await MapDishesWithRatingsAsync(dishes);

                return Ok(dishDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDishes");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Internal server error", detail = ex.Message });
            }
        }

        private async Task<IEnumerable<DishDto>> MapDishesWithRatingsAsync(IEnumerable<Models.Dish> dishes)
        {
            var dishDtos = new List<DishDto>();
            foreach (var dish in dishes)
            {
                var dto = _mapper.Map<DishDto>(dish);
                var rating = await _ratingRepository.GetDishRatingAsync(dish.Id);
                if (rating != null)
                    dto.Rating = rating.AverageRating;

                dishDtos.Add(dto);
            }
            return dishDtos;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DishDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDish(Guid id)
        {
            try
            {
                var dish = _dishRepositry.GetDish(id);
                if (dish == null)
                    return NotFound();

                var dto = _mapper.Map<DishDto>(dish);
                var rating = await _ratingRepository.GetDishRatingAsync(id);
                if (rating != null)
                    dto.Rating = rating.AverageRating;

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDish");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Internal server error", detail = ex.Message });
            }
        }

        [HttpGet("dish/{dishId}/canrate")]
        public async Task<IActionResult> CanRateDish(Guid dishId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return BadRequest("Invalid user ID.");

            try
            {
                var canRate = await _ratingRepository.CanUserRateDishAsync(userId, dishId);
                return Ok(canRate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CanRateDish");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Internal server error", detail = ex.Message });
            }
        }

        [HttpPost("dish/{dishId}/rate/{score}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RatingDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateRating(Guid dishId, int score)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Invalid user ID when creating rating.");
                    return BadRequest("Invalid user ID.");
                }

                if (score < 1 || score > 5)
                {
                    _logger.LogWarning($"Invalid score value {score} provided when creating a rating for dish with ID {dishId}");
                    return BadRequest("The score must be between 1 and 5.");
                }

                var createRatingDto = new CreateRatingDTO { Score = score };

                var rating = await _ratingRepository.CreateRatingAsync(createRatingDto, userId, dishId);
                return Ok(rating);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning($"Error creating rating for dish with ID {dishId} by user with id {User.FindFirstValue(ClaimTypes.NameIdentifier)}.");
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Invalid rating provided when creating rating for dish with ID {dishId} by user with id {User.FindFirstValue(ClaimTypes.NameIdentifier)}.");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating rating for dish with ID {dishId} by user with id {User.FindFirstValue(ClaimTypes.NameIdentifier)}.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Internal server error", detail = ex.Message });
            }
        }
    }
}



