using CarDiagnostics.Models;
using CarDiagnostics.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

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

        // ✅ שליחת בעיה חדשה למערכת עם בדיקת תקינות הקלט
        [HttpPost("submitProblem")]
        public async Task<IActionResult> SubmitProblem([FromBody] Car car)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return await _carService.SubmitProblemAsync(
                car.Username,
                car.Email,
                car.Company,
                car.Model,
                car.Year,
                car.ProblemDescription
            );
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
    }
}
