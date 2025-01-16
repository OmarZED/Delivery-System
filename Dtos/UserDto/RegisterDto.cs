using System.ComponentModel.DataAnnotations;
using WebApplication3.Models.Enum;

namespace WebApplication3.Dtos.UserDto
{
    public class RegisterDto
    {
        [Required]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string? EmailAddress { get; set; }
        [Required]
        public string? Password { get; set; }
        [Required]
        public DateOnly BirthDate { get; set; }

        public string Address { get; set; }

        public Gender Gender { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
    }
}
