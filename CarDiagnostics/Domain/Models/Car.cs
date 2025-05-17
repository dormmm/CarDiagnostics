using System.ComponentModel.DataAnnotations;

namespace CarDiagnostics.Models
{
    public class Car
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Company { get; set; }

        [Required]
        public string Model { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public string ProblemDescription { get; set; }

        // ❌ הסר את [Required]
        public string? AIResponse { get; set; }  // הפוך ל-nullable

        public string? LicensePlate { get; set; } // ✅ מספר רכב – אופציונלי
    }
}
