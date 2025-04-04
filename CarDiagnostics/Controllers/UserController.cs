using Microsoft.AspNetCore.Mvc;
using CarDiagnostics.Services;
using CarDiagnostics.Models;
using System.Threading.Tasks;
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
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _userService.RegisterAsync(request.Username, request.Password, request.Email);
            return Ok(new { Message = "User registered successfully" });
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateUserProfile(int id, [FromBody] UpdateUserProfileRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _userService.UpdateUserProfileAsync(id, request.Username, request.Email, request.Password);
            return Ok(new { Message = "User profile updated successfully" });
        }
    }
}
