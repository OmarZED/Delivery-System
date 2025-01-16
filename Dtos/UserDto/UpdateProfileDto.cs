using System.ComponentModel.DataAnnotations;
using WebApplication3.Models.Enum;

namespace WebApplication3.Dtos.UserDto
{
    public class UpdateProfileDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public DateOnly BirthDate { get; set; }


        [Required]
        public string Address { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        public Gender Gender { get; set; }
    }
}
