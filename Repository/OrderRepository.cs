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
    }
}
