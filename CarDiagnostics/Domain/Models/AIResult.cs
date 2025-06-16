namespace CarDiagnostics.Models
{
    public class AIResult
    {
        public string AIResponse { get; set; } = string.Empty;

        public string? Severity { get; set; } // Low / Medium / High

        public string? EstimatedCost { get; set; } // e.g., "500-700 ₪"

        public List<string>? Links { get; set; } // e.g., URLs to external guides
    }
}
