using System;
using CarDiagnostics.Models;  // הוספת ה-using למחלקת Car


namespace CarDiagnostics.Models
{
 public class Car
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty; // ✅ הוספת שם משתמש
    public string Email { get; set; } = string.Empty;    // ✅ הוספת אימייל
    public string Company { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string ProblemDescription { get; set; } = string.Empty;
}

}
