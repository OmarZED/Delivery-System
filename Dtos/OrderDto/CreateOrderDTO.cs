using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Dtos.OrderDto
{
    public class CreateOrderDTO
    {
        [Required]
        public DateTime DeliveryTime { get; set; }

        [Required]
        public string Address { get; set; }
    }
}
