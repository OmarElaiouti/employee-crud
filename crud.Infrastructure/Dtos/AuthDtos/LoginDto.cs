using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace crud.Infrastructure.Dtos.AuthDtos
{
    public class LoginDto
    {
        [DefaultValueAttribute("")]
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [DefaultValueAttribute("")]
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
