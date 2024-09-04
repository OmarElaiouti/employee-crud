using System.ComponentModel.DataAnnotations;

namespace crud.Infrastructure.Dtos.AuthDtos
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "username is required")]
        public string Username { get; set; }


        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }


        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and confirm password do not match")]
        public string ConfirmPassword { get; set; }
        public bool AgreeToTerms { get; set; }
    }
}
