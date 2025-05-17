using CarDiagnostics.Models;
using CarDiagnostics.Repository;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarDiagnostics.Domain.Interfaces;

namespace CarDiagnostics.Services
{
    public class CarService
    {
        private readonly AIService _aiService;
        private readonly IUserRepository _userRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ICarsCallsRepository _carsCallsRepository;

        public CarService(
            AIService aiService,
            IUserRepository userRepository,
            IVehicleRepository vehicleRepository,
            ICarsCallsRepository carsCallsRepository)
        {
            _aiService = aiService;
            _userRepository = userRepository;
            _vehicleRepository = vehicleRepository;
            _carsCallsRepository = carsCallsRepository;
        }

        public async Task<List<string>> GetAllCarCompaniesAsync()
        {
            var data = await _vehicleRepository.GetAllVehiclesAsync();
            return data.Keys.ToList();
        }

        public async Task<List<string>> GetCarModelsByCompanyAsync(string company)
        {
            return await _vehicleRepository.GetModelsByCompanyAsync(company);
        }

        public async Task<bool> IsCompanyExistsAsync(string company)
        {
            return await _vehicleRepository.IsCompanyExistsAsync(company);
        }

        public async Task<bool> IsModelExistsAsync(string company, string model)
        {
            // עדיין משאיר את זה לשימושים פנימיים רגילים
            var companyModels = await _vehicleRepository.GetModelsByCompanyAsync(company);
            return companyModels.Contains(model);
        }

        public async Task<IActionResult> SubmitProblemAsync(string username, string email, string company, string model, int year, string problemDescription)
        {
            if (!await _userRepository.IsValidUserAsync(username, email))
                return new BadRequestObjectResult("User not found in the system.");

            if (!await _vehicleRepository.IsModelExistsNormalizedAsync(company, model))
                return new BadRequestObjectResult("Company or model not found in the system.");

            var diagnosis = await _aiService.GetDiagnosisAsync(company, model, year, problemDescription);

            var carCall = new Car
            {
                Username = username,
                Email = email,
                Company = company,
                Model = model,
                Year = year,
                ProblemDescription = problemDescription,
                AIResponse = diagnosis
            };

            var existingCalls = await _carsCallsRepository.ReadCallsAsync();
            existingCalls.Add(carCall);
            await _carsCallsRepository.SaveCallsAsync(existingCalls);

            return new OkObjectResult(new { Message = "Problem submitted successfully!", AI_Diagnosis = diagnosis });
        }

        public async Task<string?> GetProblemSolutionAsync(string username, string email, string company, string model, int year, string problemDescription, string? licensePlate = null)
{
    if (!await _userRepository.IsValidUserAsync(username, email))
        return null;

    var isModelExists = await _vehicleRepository.IsModelExistsNormalizedAsync(company, model);

    if (!isModelExists)
    {
        Console.WriteLine($"⚠️ דגם לא נמצא במערכת שלך: {company} / {model} – ממשיכים בכל זאת.");
        // אתה יכול גם לרשום ל־log אם יש לך injected ILogger
    }

    var solution = await _aiService.GetDiagnosisAsync(company, model, year, problemDescription);

    var carCall = new Car
    {
        Username = username,
        Email = email,
        Company = company,
        Model = model,
        Year = year,
        ProblemDescription = problemDescription,
        AIResponse = solution,
        LicensePlate = licensePlate // ✅ שמירה בקובץ אם יש
    };

    var existingCalls = await _carsCallsRepository.ReadCallsAsync();
    existingCalls.Add(carCall);
    await _carsCallsRepository.SaveCallsAsync(existingCalls);

    return solution;
}

    }
}
