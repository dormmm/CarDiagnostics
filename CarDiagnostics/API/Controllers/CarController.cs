using CarDiagnostics.Models;
using CarDiagnostics.Services;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> GetCarCompanies()
        {
            var companies = await _carService.GetAllCarCompaniesAsync();
            return Ok(companies);
        }

        [HttpGet("plate/{plate}")]
public IActionResult GetCarByPlate(string plate, [FromServices] LicensePlateService licenseService)
{
   var data = licenseService.GetCarByPlate(plate);

    if (data == null)
        return NotFound("רכב לא נמצא");

    return Ok(data);
}


        // ✅ הצגת כל הדגמים של חברה מסוימת
        [HttpGet("getCarModels/{company}")]
        public async Task<IActionResult> GetCarModels(string company)
        {
            if (!await _carService.IsCompanyExistsAsync(company))
            {
                return NotFound(new { Message = "Company not found in the system." });
            }

            var models = await _carService.GetCarModelsByCompanyAsync(company);
            return Ok(models);
        }
    }
}
