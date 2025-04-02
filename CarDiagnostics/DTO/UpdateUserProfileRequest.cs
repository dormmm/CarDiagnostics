using System.ComponentModel.DataAnnotations;

namespace CarDiagnostics.DTO
{
    public class UpdateUserProfileRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters long")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        // הסיסמה היא אופציונלית לעדכון, לכן נשתמש רק ב- MinLength אם נשלחת
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; }
    }
}
