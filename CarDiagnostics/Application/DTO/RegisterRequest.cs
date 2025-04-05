using System.ComponentModel.DataAnnotations;

namespace CarDiagnostics.DTO
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(4, ErrorMessage = "Password must be at least 4 characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
    }
}
