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

        public string? AIResponse { get; set; }  // תשובת הבינה המלאכותית

        public string? LicensePlate { get; set; }  // מספר רכב (אופציונלי)

        public Dictionary<string, string>? FollowUp { get; set; }  // שאלות ותשובות

        public List<string>? FollowUpQuestions { get; set; }  // רשימת השאלות שנשאלו

        public string? Severity { get; set; }  // דרגת סיכון (Low / Medium / High)

        public string? EstimatedCost { get; set; }  // הערכת עלות

        public List<string>? Links { get; set; }  // קישורים למקורות מידע
    }
}
