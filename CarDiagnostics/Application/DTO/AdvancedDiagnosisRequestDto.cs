namespace CarDiagnostics.Application.DTO
{
    public class AdvancedDiagnosisRequestDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string? LicensePlate { get; set; } // אופציונלי
        public string? Company { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
        public string ProblemDescription { get; set; }

        // תשובות במבנה של שאלה → תשובה (אם המשתמש עונה כך)
        public Dictionary<string, string>? FollowUpAnswers { get; set; }

        // תשובות לפי סדר בלבד (אם המשתמש עונה תשובות קצרות לפי הסדר)
        public List<string>? Answers { get; set; }
    }
}
