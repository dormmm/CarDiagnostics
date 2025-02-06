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

        // ✅ הצגת כל החברות הקיימות במערכת
        [HttpGet("getCarCompanies")]
        public IActionResult GetCarCompanies()
        {
            var companies = _carService.GetAllCarCompanies();
            return Ok(companies);
        }

        // ✅ הצגת כל הדגמים של חברה מסוימת
        [HttpGet("getCarModels/{company}")]
        public IActionResult GetCarModels(string company)
        {
            if (!_carService.IsCompanyExists(company))
            {
                return NotFound(new { Message = "Company not found in the system." });
            }

            var models = _carService.GetCarModelsByCompany(company);
            return Ok(models);
        }

        // ✅ הגשת קריאת שירות לרכב
        [HttpPost("submitProblem")]
        public IActionResult SubmitProblem([FromBody] Car car)
        {
            try
            {
                // בדיקה אם החברה קיימת
                if (!_carService.IsCompanyExists(car.Company))
                {
                    return BadRequest(new { Message = "Company not found in the system." });
                }

                // בדיקה אם הדגם קיים תחת החברה
                if (!_carService.IsCarModelExists(car.Company, car.Model))
                {
                    return BadRequest(new { Message = "Car model not found under this company." });
                }

                // קריאה לשירות הוספת קריאה למערכת
                _carService.SubmitProblem(car.Company, car.Model, car.Year, car.ProblemDescription);

                return Ok(new { Message = "Problem submitted successfully!" });
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
