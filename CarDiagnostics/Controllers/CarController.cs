using CarDiagnostics.Models;
using CarDiagnostics.Services;
using Microsoft.AspNetCore.Mvc;

namespace CarDiagnostics.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarController : ControllerBase
    {
        private readonly CarService _carService;

        public CarController(CarService carService)
        {
            _carService = carService;
        }

        // הוספת רכב עם קישור למשתמש
        [HttpPost("submitProblem")]
        public IActionResult SubmitProblem([FromBody] Car car)
        {
            try
            {
                // ה-UserId יכול להגיע מה-Session או Token. כרגע אנחנו מניחים 1.
                int userId = car.UserId;  // השתמש ב-UserId שנשלח מה-Postman

                _carService.SubmitProblem(userId, car.Company, car.Model, car.Year, car.ProblemDescription);
                return Ok(new { Message = "Problem submitted successfully!" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
