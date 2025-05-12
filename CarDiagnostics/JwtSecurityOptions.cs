using Microsoft.IdentityModel.Tokens;

// ======= JwtSecurityOptions.cs =======
namespace CarDiagnostics.Configuration
{
    public class JwtSecurityOptions
    {
        public const string SECTION = "Jwt";

        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int TokenValidityMinutes { get; set; } = 15;
        public string Algorithm { get; set; } = "HS256";
    }
}
