using AutoMapper;
using WebApplication3.Dtos.OrderDto;
using WebApplication3.Interface;
using WebApplication3.Maping;
using WebApplication3.Models.Enum;
using WebApplication3.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApplication3.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IBasketRepository _basketRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(ApplicationDbContext context, IBasketRepository basketRepository, IMapper mapper, ILogger<OrderRepository> logger)
        {
            _context = context;
            _basketRepository = basketRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// Creates a new order based on the provided DTO.
        public async Task<OrderDTO?> CreateOrderAsync(CreateOrderDTO createOrderDto, string userId)
        {
            try
            {
                var basket = await _basketRepository.GetUserBasketAsync(userId);
                if (basket == null || !basket.BasketItems.Any())
                {
                    _logger.LogWarning($"Basket not found or is empty when creating order for user with ID {userId}.");
                    throw new Exception("Basket not found, or basket is empty");
                }

                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    DeliveryTime = createOrderDto.DeliveryTime,
                    OrderTime = DateTime.UtcNow,
                    Status = OrderStatus.InProcess,
                    Address = createOrderDto.Address,
                    UserId = userId,
                };
                _context.Orders.Add(order);


                var orderItems = basket.BasketItems.Select(basketItem => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    DishId = basketItem.DishId,
                    Name = basketItem.Name,
                    Price = basketItem.Price,
                    Amount = basketItem.Amount,
                    Image = basketItem.Image
                }).ToList();

                _context.OrderItems.AddRange(orderItems);

                order.Price = order.TotalPrice;

                await _context.SaveChangesAsync();
                await _basketRepository.ClearBasketAsync(userId);
                return _mapper.Map<OrderDTO>(order);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating order for user with ID {userId}.");
                return null;
            }

        }

        /// Retrieves all orders placed by a specific user.
        public async Task<List<OrderInfoDTO>> GetUserOrdersAsync(string userId)
        {
            try
            {
                var orders = await _context.Orders.Include(o => o.OrderItems)
                  .Where(x => x.UserId == userId).ToListAsync();
                return _mapper.Map<List<OrderInfoDTO>>(orders);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user orders for user with ID {userId}.");
                return new List<OrderInfoDTO>();
            }

        }
        /// Retrieves an order by its ID and user ID.
        public async Task<OrderDTO?> GetOrderByIdAsync(Guid orderId, string userId)
        {
            try
            {
                var order = await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Dish)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

                if (order == null)
                {
                    _logger.LogWarning($"Order with ID {orderId} not found for user with ID {userId}.");
                    return null;
                }
                return _mapper.Map<OrderDTO>(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting order with ID {orderId} for user with ID {userId}.");
                return null;
            }

        }

        /// Sets the status of an order to "Delivered".
        public async Task<bool> SetOrderStatusToDeliveredAsync(Guid orderId, string userId)
        {
            try
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

                if (order == null)
                {
                    _logger.LogWarning($"Order with ID {orderId} not found for user ID {userId} when setting status to delivered.");
                    return false;
                }
                order.Status = OrderStatus.Delivered;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting order status to delivered for order with ID {orderId} for user with ID {userId}.");
                return false;
            }
        }
    }
}
