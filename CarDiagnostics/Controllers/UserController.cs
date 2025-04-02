using Microsoft.AspNetCore.Mvc;
using CarDiagnostics.Services;
using CarDiagnostics.DTO;
using System;

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
        public IActionResult GetUsers()
        {
            var users = _userService.GetAllUsers();
            return Ok(users);
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            _userService.Register(request.Username, request.Password, request.Email);
            return Ok(new { Message = "User registered successfully" });
        }

        [HttpPut("update/{id}")]
        public IActionResult UpdateUserProfile(int id, [FromBody] UpdateUserProfileRequest request)
        {
            try
            {
                _userService.UpdateUserProfile(id, request.Username, request.Email, request.Password);
                return Ok(new { Message = "User profile updated successfully" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An unexpected error occurred." });
            }
        }
    }
}
