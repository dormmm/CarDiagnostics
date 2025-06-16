using CarDiagnostics.Models;
using CarDiagnostics.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CarDiagnostics.Application.DTO;

namespace CarDiagnostics.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarController : ControllerBase
    {
        private readonly CarService _carService;
        private readonly LicensePlateService _licensePlateService;

        public CarController(CarService carService, LicensePlateService licensePlateService)
        {
            _carService = carService;
            _licensePlateService = licensePlateService;
        }

        [HttpPost("submitProblem")]
        public async Task<IActionResult> SubmitProblem([FromBody] Car car)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return await _carService.SubmitProblemAsync(
                car.Username,
                car.Email,
                car.Company,
                car.Model,
                car.Year,
                car.ProblemDescription
            );
        }

        [HttpPost("submitProblemByPlate")]
        public async Task<IActionResult> SubmitProblemByPlate([FromBody] SubmitProblemByPlateDto dto)
        {
            var plateInfo = _licensePlateService.GetCarByPlate(dto.LicensePlate);
            if (plateInfo == null)
                return NotFound("רכב לא נמצא לפי מספר הרישוי.");

            var manufacturer = plateInfo["manufacturer"]?.ToString();
            var model = plateInfo["model"]?.ToString();
            var year = int.TryParse(plateInfo["year"]?.ToString(), out int y) ? y : 0;

            var solution = await _carService.GetProblemSolutionAsync(
                dto.Username,
                dto.Email,
                manufacturer,
                model,
                year,
                dto.ProblemDescription,
                dto.LicensePlate
            );

            if (solution == null)
                return NotFound("בעיה לא נמצאה או משתמש/רכב לא תקין.");

            var fullCarData = new Dictionary<string, string>
            {
                ["licensePlate"] = dto.LicensePlate // הוסף את מספר הרישוי בשורה הראשונה
            };

            // הוסף את שאר המידע מהרכב
            foreach (var kvp in plateInfo)
            {
                fullCarData[kvp.Key] = kvp.Value?.ToString() ?? "";
            }

            // הוסף את התיאור והפתרון
            fullCarData["problemDescription"] = dto.ProblemDescription;
            fullCarData["solution"] = solution;

            return Ok(fullCarData);
        }

        [HttpGet("getCarCompanies")]
        public async Task<IActionResult> GetCarCompanies()
        {
            var companies = await _carService.GetAllCarCompaniesAsync();
            return Ok(companies);
        }

        [HttpGet("getCarModels/{company}")]
        public async Task<IActionResult> GetCarModels(string company)
        {
            if (!await _carService.IsCompanyExistsAsync(company))
                return NotFound(new { Message = "Company not found in the system." });

            var models = await _carService.GetCarModelsByCompanyAsync(company);
            return Ok(models);
        }

        [HttpGet("plate/{plate}")]
        public IActionResult GetCarByPlate(string plate, [FromServices] LicensePlateService licenseService)
        {
            // בדיקת תקינות: מספר רכב חייב להיות 7 או 8 ספרות בלבד
            if (string.IsNullOrWhiteSpace(plate) ||
                !(plate.Length == 7 || plate.Length == 8) ||
                !plate.All(char.IsDigit))
            {
                return BadRequest("מספר רכב חייב להכיל 7 או 8 ספרות בלבד.");
            }

            var data = licenseService.GetCarByPlate(plate);
            if (data == null)
                return NotFound("רכב לא נמצא");

            return Ok(data);
        }

        [HttpPost("advancedDiagnosis")]
public async Task<IActionResult> AdvancedDiagnosis([FromBody] AdvancedDiagnosisRequestDto dto)
{
    string? company = dto.Company;
    string? model = dto.Model;
    int? year = dto.Year;

    if (!string.IsNullOrEmpty(dto.LicensePlate))
    {
        var plateInfo = _licensePlateService.GetCarByPlate(dto.LicensePlate);
        if (plateInfo == null)
            return NotFound("רכב לא נמצא לפי מספר הרישוי.");

        company = plateInfo["manufacturer"]?.ToString();
        model = plateInfo["model"]?.ToString();
        year = int.TryParse(plateInfo["year"]?.ToString(), out var parsedYear) ? parsedYear : null;
    }

    if (string.IsNullOrEmpty(company) || string.IsNullOrEmpty(model) || year == null)
        return BadRequest("יש לספק או מספר רישוי תקין או פרטי רכב מלאים.");

    var result = await _carService.RunAdvancedDiagnosisAsync(
        dto.Username,
        dto.Email,
        company!,
        model!,
        year.Value,
        dto.ProblemDescription,
        dto.FollowUpAnswers ?? new Dictionary<string, string>(),
        dto.Answers, // ✅ זה השדה החדש מסוג List<string>
        dto.LicensePlate
    );

    return Ok(result);
}

    }
}
