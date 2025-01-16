using WebApplication3.Models.Enum;

namespace WebApplication3.Dtos.OrderDto
{
    public class OrderDTO
    {
        public Guid Id { get; set; }
        public DateTime DeliveryTime { get; set; }
        public DateTime OrderTime { get; set; }
        public OrderStatus Status { get; set; }
        public double Price { get; set; }
        public string Address { get; set; }
        public List<OrderItemDTO> OrderItems { get; set; }
    }
}
