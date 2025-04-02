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

        [HttpPost("submitProblem")]
public async Task<IActionResult> SubmitProblem([FromBody] Car car)
{
    if (car == null)
    {
        return BadRequest(new { Error = "Invalid request: car data is missing." });
    }

    try
    {
        bool companyExists = _carService.IsCompanyExists(car.Company);
        if (!companyExists)
        {
            return NotFound(new { Error = $"Company '{car.Company}' not found in database." });
        }

        bool modelExists = _carService.IsModelExists(car.Company, car.Model);
        if (!modelExists)
        {
            return NotFound(new { Error = $"Model '{car.Model}' not found for company '{car.Company}'." });
        }

        // ✅ כאן מחזירים את התוצאה מהשירות – כולל האבחנה!
        return await _carService.SubmitProblemAsync(
            car.Username,
            car.Email,
            car.Company,
            car.Model,
            car.Year,
            car.ProblemDescription
        );
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { Error = "An unexpected error occurred while submitting the problem." });
    }
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
