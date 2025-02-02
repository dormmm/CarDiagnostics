using System;
using CarDiagnostics.Models;  // הוספת ה-using למחלקת Car


namespace CarDiagnostics.Models
{
   public class Car
{
    public int Id { get; set; }            // מזהה הרכב
    public string Company { get; set; }    // שם החברה (Toyota, Honda, וכו')
    public string Model { get; set; }      // דגם הרכב
    public int Year { get; set; }          // שנת ייצור הרכב
    public string ProblemDescription { get; set; }  // תיאור הבעיה ברכב
    public int UserId { get; set; }        // מזהה המשתמש שאליו שייך הרכב
}
}
