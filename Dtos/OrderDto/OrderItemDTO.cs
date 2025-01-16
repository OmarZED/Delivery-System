namespace WebApplication3.Dtos.OrderDto
{
    public class OrderItemDTO
    {
        public Guid Id { get; set; }
        public Guid DishId { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public string Image { get; set; }
        public int Amount { get; set; }
    }
}
