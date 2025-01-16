using Microsoft.AspNetCore.Identity;
using WebApplication3.Models.Enum;

namespace WebApplication3.Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
        public DateOnly BirthDate { get; set; }
        public string Address { get; set; }
        public Gender Gender { get; set; }

        public Basket Basket { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();

    }
}
