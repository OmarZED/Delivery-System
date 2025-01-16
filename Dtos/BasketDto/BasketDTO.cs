namespace WebApplication3.Dtos.BasketDto
{
    public class BasketDTO
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public List<BasketItemDTO> BasketItems { get; set; } = new List<BasketItemDTO>();
    }
}
