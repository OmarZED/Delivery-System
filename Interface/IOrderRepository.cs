using WebApplication3.Dtos.OrderDto;

namespace WebApplication3.Interface
{
    public interface IOrderRepository
    {
        /// Creates a new order for the specified user.
        Task<OrderDTO?> CreateOrderAsync(CreateOrderDTO createOrderDto, string userId);


        /// Retrieves a list of all orders placed by a specific user.
        Task<List<OrderInfoDTO>> GetUserOrdersAsync(string userId);

        /// Retrieves the full details of a specific order by its ID for a given user.
        Task<OrderDTO?> GetOrderByIdAsync(Guid orderId, string userId);

        /// Updates the status of an order to 'Delivered'
        Task<bool> SetOrderStatusToDeliveredAsync(Guid orderId, string userId);
    }
}
