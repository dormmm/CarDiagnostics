using Microsoft.AspNetCore.Mvc;
using CarDiagnostics.Models;
using CarDiagnostics.Services;
using System.Linq;
using System;
using System.Threading.Tasks;

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
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var users = await _userService.GetAllUsersAsync(); // ⚠️ מתודה אסינכרונית
            var user = users.FirstOrDefault(u =>
                u.Username == request.Username &&
                BCrypt.Net.BCrypt.Verify(request.Password, u.Password)
            );

            if (user != null)
            {
                return Ok(new { Message = "Login successful" });
            }

            return Unauthorized(new { Message = "Username or password is incorrect" });
        }
    }
}
