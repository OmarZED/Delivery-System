using WebApplication3.Models.Enum;

namespace WebApplication3.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public DateTime DeliveryTime { get; set; }
        public DateTime OrderTime { get; set; }
        public OrderStatus Status { get; set; }
        public double Price { get; set; }
        public string Address { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public double TotalPrice => OrderItems.Sum(oi => oi.Price * oi.Amount);
    }
}
