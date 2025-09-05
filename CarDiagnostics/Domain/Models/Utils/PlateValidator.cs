using System.Linq;

namespace CarDiagnostics.Domain.Utils
{
    public static class PlateValidator
    {
        // תקף אם אחרי ניקוי נשארות 7 או 8 ספרות
        public static bool IsValid(string? plate)
        {
            if (string.IsNullOrWhiteSpace(plate)) return false;
            var digitsOnly = new string(plate.Where(char.IsDigit).ToArray());
            return digitsOnly.Length is 7 or 8 && digitsOnly.All(char.IsDigit);
        }

        // מנרמל: מחזיר רק ספרות (מוריד מקפים/רווחים/תווים)
        public static string Normalize(string? plate)
        {
            return new string((plate ?? string.Empty).Where(char.IsDigit).ToArray());
        }
    }
}
