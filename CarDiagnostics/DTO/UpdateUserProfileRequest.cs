namespace CarDiagnostics.DTO
{
    public class UpdateUserProfileRequest
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } // אופציונלי לעדכון
    }
}
