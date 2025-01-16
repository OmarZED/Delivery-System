using WebApplication3.Dtos.OrderDto;

namespace WebApplication3.Interface
{
    public interface IOrderRepository
    {
        Task<OrderDTO?> CreateOrderAsync(CreateOrderDTO createOrderDto, string userId);
        Task<List<OrderInfoDTO>> GetUserOrdersAsync(string userId);
        Task<OrderDTO?> GetOrderByIdAsync(Guid orderId, string userId);
        Task<bool> SetOrderStatusToDeliveredAsync(Guid orderId, string userId);
    }
}
