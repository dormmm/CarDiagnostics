using Microsoft.AspNetCore.Mvc;
using CarDiagnostics.Models;
using CarDiagnostics.Services;
using System.Linq;

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
            // קבלת המשתמשים דרך IUserService
            var users = _userService.GetAllUsers();

            // בדיקה אם שם המשתמש והסיסמה תואמים
            var user = users.FirstOrDefault(u => u.Username == request.Username && u.Password == request.Password);

            if (user != null)
            {
                return Ok(new { Message = "Login successful" });
            }

            return Unauthorized(new { Message = "Username or password is incorrect" });
        }
    }
}
