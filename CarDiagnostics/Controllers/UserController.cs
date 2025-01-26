using Microsoft.AspNetCore.Mvc;
using CarDiagnostics.Services;
using CarDiagnostics.Models;
using System.Collections.Generic;
using Newtonsoft.Json;
using CarDiagnostics.DTO;  // ייבוא המודלים מ-DTO




namespace CarDiagnostics.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // הצגת כל המשתמשים
        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            var users = _userService.GetAllUsers();  // מקבל את כל המשתמשים מהשירות
            return Ok(users);  // מחזיר את רשימת המשתמשים
        }

        // רישום משתמש חדש
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            _userService.Register(request.Username, request.Password, request.Email);  // רושם את המשתמש החדש
            return Ok(new { Message = "User registered successfully" });  // מחזיר הודעה שהמשתמש נרשם בהצלחה
        }

        // עדכון פרטי משתמש
        [HttpPut("update/{id}")]
        public IActionResult UpdateUserProfile(int id, [FromBody] UpdateUserProfileRequest request)
        {
            _userService.UpdateUserProfile(id, request.Username, request.Email, request.Password);  // מעדכן את פרטי המשתמש
            return Ok(new { Message = "User profile updated successfully" });  // מחזיר הודעה שהפרופיל עודכן בהצלחה
        }
    }
}
