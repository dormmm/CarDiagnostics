using Microsoft.AspNetCore.Mvc;
using CarDiagnostics.Services;
using CarDiagnostics.Models;
using System.Collections.Generic;
using Newtonsoft.Json;
using CarDiagnostics.DTO;

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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _userService.Register(request.Username, request.Password, request.Email);
            return Ok(new { Message = "User registered successfully" });
        }

        [HttpPut("update/{id}")]
        public IActionResult UpdateUserProfile(int id, [FromBody] UpdateUserProfileRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _userService.UpdateUserProfile(id, request.Username, request.Email, request.Password);
            return Ok(new { Message = "User profile updated successfully" });
        }
    }
}
