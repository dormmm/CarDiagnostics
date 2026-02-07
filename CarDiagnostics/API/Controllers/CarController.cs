using CarDiagnostics.Models;
using CarDiagnostics.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CarDiagnostics.Application.DTO;
using System.Collections.Generic;
using System.Linq;

namespace CarDiagnostics.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarController : ControllerBase
    {
        private readonly CarService _carService;
        private readonly LicensePlateService _licensePlateService;
        private readonly AIService _aiService; // ✅ חדש - בשביל Severity/EstimatedCost

        // ✅ עדכון: הוספנו AIService ל-DI
        public CarController(CarService carService, LicensePlateService licensePlateService, AIService aiService)
        {
            _carService = carService;
            _licensePlateService = licensePlateService;
            _aiService = aiService;
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
            var plateInfo = await _licensePlateService.GetCarByPlateAsync(dto.LicensePlate);

            if (plateInfo == null)
                return NotFound("רכב לא נמצא לפי מספר הרישוי.");

            var manufacturer = plateInfo.ContainsKey("manufacturer") ? plateInfo["manufacturer"]?.ToString() : null;
            var model = plateInfo.ContainsKey("model") ? plateInfo["model"]?.ToString() : null;
            var year = int.TryParse(plateInfo.ContainsKey("year") ? plateInfo["year"]?.ToString() : null, out int y) ? y : 0;

            var solution = await _carService.GetProblemSolutionAsync(
                dto.Username,
                dto.Email,
                manufacturer ?? "",
                model ?? "",
                year,
                dto.ProblemDescription,
                dto.LicensePlate
            );

            if (solution == null)
                return NotFound("בעיה לא נמצאה או משתמש/רכב לא תקין.");

            // ✅ חילוץ חומרה + מחיר מתוך הטקסט (בדיוק כמו במתקדם)
            var severity = _aiService.ExtractSeverityPublic(solution);
            var estimatedCost = _aiService.ExtractEstimatedCostPublic(solution);

            var fullCarData = new Dictionary<string, string>
            {
                ["licensePlate"] = dto.LicensePlate // מספר רישוי בשורה הראשונה
            };

            // הוסף את שאר המידע מהרכב
            foreach (var kvp in plateInfo)
            {
                fullCarData[kvp.Key] = kvp.Value?.ToString() ?? "";
            }

            // הוסף את התיאור והפתרון
            fullCarData["problemDescription"] = dto.ProblemDescription;
            fullCarData["solution"] = solution;

            // ✅ אם לא קיים – לא מוסיפים בכלל (כדי לא לשבור UI)
            if (!string.IsNullOrWhiteSpace(severity))
                fullCarData["Severity"] = severity;

            if (!string.IsNullOrWhiteSpace(estimatedCost))
                fullCarData["EstimatedCost"] = estimatedCost;

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
        public async Task<IActionResult> GetCarByPlate(string plate, [FromServices] LicensePlateService licenseService)
        {
            // בדיקת תקינות: מספר רכב חייב להיות 7 או 8 ספרות בלבד
            if (string.IsNullOrWhiteSpace(plate) ||
                !(plate.Length == 7 || plate.Length == 8) ||
                !plate.All(char.IsDigit))
            {
                return BadRequest("מספר רכב חייב להכיל 7 או 8 ספרות בלבד.");
            }

            var data = await licenseService.GetCarByPlateAsync(plate);

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
                var plateInfo = await _licensePlateService.GetCarByPlateAsync(dto.LicensePlate);

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
