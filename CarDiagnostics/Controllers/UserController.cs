using Microsoft.AspNetCore.Mvc;
using CarDiagnostics.Services;
using System.Collections.Generic;
using CarDiagnostics.DTOs;
using CarDiagnostics.Models;


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

        [HttpGet("users")]
        public IEnumerable<User> GetUsers()
        {
            // החזר רשימה של משתמשים
            return _userService.GetAllUsers();
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            try
            {
                _userService.Register(request.Username, request.Password, request.Email);
                return Ok(new { Message = "User registered successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
        }
    }
}
