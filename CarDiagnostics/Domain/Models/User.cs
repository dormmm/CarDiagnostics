using CarDiagnostics.Models;
using System;
using System.Collections.Generic;


namespace CarDiagnostics.Models

{
    public class User
    {
        public int Id { get; set; } // מזהה ייחודי לכל משתמש
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

        public List<Car> Cars { get; set; }  // רשימת רכבים ששייכים למשתמש
    }
}
