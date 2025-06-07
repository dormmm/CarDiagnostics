using CarDiagnostics.Models;
using CarDiagnostics.Repository;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ProblemTopicService _problemTopicService;
        private readonly ManualLinkService _manualLinkService;

        public CarService(
            AIService aiService,
            IUserRepository userRepository,
            IVehicleRepository vehicleRepository,
            ICarsCallsRepository carsCallsRepository,
            ProblemTopicService problemTopicService,
            ManualLinkService manualLinkService)
        {
            _aiService = aiService;
            _userRepository = userRepository;
            _vehicleRepository = vehicleRepository;
            _carsCallsRepository = carsCallsRepository;
            _problemTopicService = problemTopicService;
            _manualLinkService = manualLinkService;
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
            var companyModels = await _vehicleRepository.GetModelsByCompanyAsync(company);
            return companyModels.Contains(model);
        }

       public async Task<IActionResult> SubmitProblemAsync(string username, string email, string company, string model, int year, string problemDescription)
{
    if (!await _userRepository.IsValidUserAsync(username, email))
        return new BadRequestObjectResult("User not found in the system.");

    if (!await _vehicleRepository.IsModelExistsNormalizedAsync(company, model))
        return new BadRequestObjectResult("Company or model not found in the system.");

    var solution = await GetProblemSolutionAsync(username, email, company, model, year, problemDescription);

    return new OkObjectResult(new
    {
        Message = "Problem submitted successfully!",
        AI_Diagnosis = solution
    });
}

public async Task<string?> GetProblemSolutionAsync(
    string username,
    string email,
    string company,
    string model,
    int year,
    string problemDescription,
    string? licensePlate = null)
{
    if (!await _userRepository.IsValidUserAsync(username, email))
        return null;

    var isModelExists = await _vehicleRepository.IsModelExistsNormalizedAsync(company, model);
    if (!isModelExists)
    {
        Console.WriteLine($"⚠️ דגם לא נמצא במערכת שלך: {company} / {model} – ממשיכים בכל זאת.");
    }

    // שלב 1: קבלת פתרון GPT
    var solution = await _aiService.GetDiagnosisAsync(company, model, year, problemDescription);

    // שלב 2: ניתוח נושא ומילים נרדפות
    var topicData = await _problemTopicService.ExtractTopicAndKeywordsAsync(problemDescription);
    Console.WriteLine($"🔍 נושא שזוהה: {topicData.topic}");
    Console.WriteLine("📚 מילים נרדפות שזוהו:");
    foreach (var word in topicData.keywords)
        Console.WriteLine($"- {word}");

    // שלב 3: חיפוש קישורים + הודעת fallback
    var (manualLinks, fallbackMessage) = _manualLinkService.FindLinks(
        company, model, year, topicData.topic, topicData.keywords);

    // הוספת fallback אם קיים
    if (!string.IsNullOrEmpty(fallbackMessage))
    {
        solution = $"⚠️ {fallbackMessage}\n\n" + solution;
    }

    // הוספת קישורים לספר הרכב
    if (manualLinks.Any())
    {
        Console.WriteLine("🔗 נמצאו קישורים מתוך ספר הרכב:");
        foreach (var entry in manualLinks)
            Console.WriteLine($"- {entry.Key}: {entry.Value}");

        solution += "\n\n📘 מידע נוסף מתוך ספר הרכב: ";
        solution += string.Join(" | ", manualLinks.Select(entry =>
            $"{entry.Key}: {entry.Value?.Replace("\n", "").Replace("\r", "").Trim()}"));
    }
    else
    {
        Console.WriteLine("❌ לא נמצאו קישורים מתאימים מתוך ספר הרכב.");
    }

    // שלב 4: שמירה לקריאה
    var carCall = new Car
    {
        Username = username,
        Email = email,
        Company = company,
        Model = model,
        Year = year,
        ProblemDescription = problemDescription,
        AIResponse = solution,
        LicensePlate = licensePlate
    };

    var existingCalls = await _carsCallsRepository.ReadCallsAsync();
    existingCalls.Add(carCall);
    await _carsCallsRepository.SaveCallsAsync(existingCalls);

    return solution;
}

    }
}
