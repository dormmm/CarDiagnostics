using CarDiagnostics.Models;
namespace CarDiagnostics.Models

{
    public class User
    {
        public int Id { get; set; } // מזהה ייחודי לכל משתמש
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
}
