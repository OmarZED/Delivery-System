using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication3.Dtos.OrderDto;
using WebApplication3.Interface;

namespace WebApplication3.Controllers
{
    [ApiController]
    [Route("api/order")]
    [Authorize] // All endpoints require authentication
    public class OrderController : ControllerBase
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IBasketRepository basketRepository, IOrderRepository orderRepository, IMapper mapper, ILogger<OrderController> logger)
        {
            _basketRepository = basketRepository;
            _orderRepository = orderRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// Creates a new order for the authenticated user.
        [HttpPost]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDTO createOrderDto)
        {
            var userId = GetAuthenticatedUserId(out var errorResult);
            if (errorResult != null) return errorResult;

            try
            {
                await _orderRepository.CreateOrderAsync(createOrderDto, userId!);
                return Ok(new { message = "Order created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating order by user with ID {userId}.");
                return BadRequest($"Error: {ex.Message}");
            }
        }

        /// Retrieves all orders placed by the authenticated user
        [HttpGet("user")]
        [ProducesResponseType(200, Type = typeof(List<OrderInfoDTO>))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetUserOrders()
        {
            var userId = GetAuthenticatedUserId(out var errorResult);
            if (errorResult != null) return errorResult;

            try
            {
                var orders = await _orderRepository.GetUserOrdersAsync(userId!);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user orders.");
                return StatusCode(500, "Internal server error");
            }
        }

        /// Retrieves a specific order by its ID.
        [HttpGet("{orderId}")]
        [ProducesResponseType(200, Type = typeof(OrderDTO))]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            var userId = GetAuthenticatedUserId(out var errorResult);
            if (errorResult != null) return errorResult;

            try
            {
                var order = await _orderRepository.GetOrderByIdAsync(orderId, userId!);
                if (order == null)
                {
                    _logger.LogWarning($"Order with ID {orderId} not found.");
                    return NotFound("Order not found");
                }
                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting order with ID {orderId}.");
                return StatusCode(500, "Internal server error");
            }
        }

        /// Sets the status of a specific order to delivered.
        [HttpPut("{orderId}/delivered")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> SetOrderStatusToDelivered(Guid orderId)
        {
            var userId = GetAuthenticatedUserId(out var errorResult);
            if (errorResult != null) return errorResult;

            try
            {
                var result = await _orderRepository.SetOrderStatusToDeliveredAsync(orderId, userId!);
                if (!result)
                {
                    _logger.LogWarning($"Order with ID {orderId} not found when setting status to delivered.");
                    return NotFound("Order not found");
                }
                return Ok(new { message = "Order status set to delivered" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting order with ID {orderId} to delivered.");
                return StatusCode(500, "Internal server error");
            }
        }

        /// 🔐 Helper method to extract authenticated user ID
        private string? GetAuthenticatedUserId(out IActionResult? errorResult)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found or invalid in the token.");
                errorResult = BadRequest("Invalid or missing user ID.");
                return null;
            }
            errorResult = null;
            return userId;
        }
    }
}