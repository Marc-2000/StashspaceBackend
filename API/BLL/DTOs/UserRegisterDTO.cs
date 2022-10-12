using System.ComponentModel.DataAnnotations;

namespace API.BLL.DTOs
{
    public class UserRegisterDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string ConfirmPassword { get; set; }

        [Required]
        public string PhoneNumber { get; set; }
    }
}
