using Microsoft.AspNetCore.Mvc;
using CarDiagnostics.Models;
using CarDiagnostics.Services;
using System.Linq;
using System;

namespace CarDiagnostics.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly IUserService _userService;

        public LoginController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                var users = _userService.GetAllUsers();
                var user = users.FirstOrDefault(u => u.Username == request.Username);

                if (user == null)
                {
                    return NotFound(new { Error = "User not found" });
                }

                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
                if (!isPasswordValid)
                {
                    return Unauthorized(new { Error = "Invalid password" });
                }

                return Ok(new
                {
                    Message = "Login successful",
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }
    }
}
