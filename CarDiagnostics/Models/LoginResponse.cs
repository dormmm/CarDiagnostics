namespace CarDiagnostics.Models
{
    public class LoginResponse
    {
        public string Username { get; set; }
        public string Token { get; set; } // ניתן להוסיף מערכת להפקת JWT במידת הצורך
    }
}
