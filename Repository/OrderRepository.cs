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
                var basket = await ValidateBasket(userId);
                if (basket == null) return null;

                var order = CreateOrderEntity(createOrderDto, userId);
                var orderItems = MapBasketItemsToOrderItems(basket.BasketItems, order.Id);

                _context.Orders.Add(order);
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

        private async Task<Basket?> ValidateBasket(string userId)
        {
            var basket = await _basketRepository.GetUserBasketAsync(userId);
            if (basket == null || !basket.BasketItems.Any())
            {
                _logger.LogWarning($"Basket not found or is empty when creating order for user with ID {userId}.");
                return null;
            }
            return basket;
        }

        private Order CreateOrderEntity(CreateOrderDTO dto, string userId)
        {
            return new Order
            {
                Id = Guid.NewGuid(),
                DeliveryTime = dto.DeliveryTime,
                OrderTime = DateTime.UtcNow,
                Status = OrderStatus.InProcess,
                Address = dto.Address,
                UserId = userId
            };
        }

        private List<OrderItem> MapBasketItemsToOrderItems(IEnumerable<BasketItem> basketItems, Guid orderId)
        {
            return basketItems.Select(item => new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                DishId = item.DishId,
                Name = item.Name,
                Price = item.Price,
                Amount = item.Amount,
                Image = item.Image
            }).ToList();
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
