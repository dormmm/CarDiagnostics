using CarDiagnostics.Models;
using CarDiagnostics.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
                // בדיקה אם הרכב קיים
                var cars = _carService.ReadCarsFromFile();
                var existingCar = cars.FirstOrDefault(c => c.Company == car.Company && c.Model == car.Model && c.Year == car.Year);

                if (existingCar == null)
                {
                    return BadRequest(new { Message = "Car not found in the system." }); // אם הרכב לא נמצא
                }

                // הוספת הבעיה לרכב
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
