using CarDiagnostics.Models;
using CarDiagnostics.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarDiagnostics.Domain.Interfaces;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text;




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
        private readonly FollowUpQuestionStore _followUpStore;
        private readonly ManualContentFetcher _manualContentFetcher;

        


      public CarService(
    AIService aiService,
    IUserRepository userRepository,
    IVehicleRepository vehicleRepository,
    ICarsCallsRepository carsCallsRepository,
    ProblemTopicService problemTopicService,
    ManualLinkService manualLinkService,
    FollowUpQuestionStore followUpStore, // ✅ הוספה
    ManualContentFetcher manualContentFetcher // ✅ הוספה
)
{
    _aiService = aiService;
    _userRepository = userRepository;
    _vehicleRepository = vehicleRepository;
    _carsCallsRepository = carsCallsRepository;
    _problemTopicService = problemTopicService;
    _manualLinkService = manualLinkService;
    _followUpStore = followUpStore; // ✅ שמירה
     _manualContentFetcher = manualContentFetcher; // ✅ שמירה
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

    var topicData = await _problemTopicService.ExtractTopicAndKeywordsAsync(problemDescription);
    Console.WriteLine($"🔍 נושא שזוהה: {topicData.topic}");
    foreach (var word in topicData.keywords)
        Console.WriteLine($"- {word}");

    // שלב 1: חיפוש קישורים
    var (manualLinks, fallbackMessage) = _manualLinkService.FindLinks(
        company, model, year, topicData.topic, topicData.keywords);

    var combinedContent = new StringBuilder();

    foreach (var entry in manualLinks)
{
    Console.WriteLine($"🔗 מנסה לשלוף תוכן עבור: {entry.Key} - {entry.Value}");
    var content = await _manualContentFetcher.FetchCleanContentAsync(entry.Value);

    if (string.IsNullOrWhiteSpace(content))
        Console.WriteLine($"⚠️ תוכן ריק מתוך {entry.Key}");
    else
        Console.WriteLine($"📄 תוכן התקבל עבור {entry.Key}, אורך: {content.Length}");

    combinedContent.AppendLine($"[From: {entry.Key}]\n{content}\n");

   // Console.WriteLine($"📄 סיכום מתוך {entry.Key}:\n{content}\n------------------------");

}


    var finalPrompt = $"""
רכב: {company} {model} {year}
תיאור תקלה: {problemDescription}

ראשית, השב על השאלה על סמך הידע הכללי שלך כמומחה לרכב.

לאחר מכן, הצג מידע רלוונטי מתוך ספר הרכב – אם יש, והפרד אותו מהחלק הקודם.

קיבלת מידע טכני מתוך ספר הרכב, והוא תואם לרכב ולבעיה שתיאר המשתמש.
על סמך מידע זה ובשילוב הידע הכללי שלך, תן אבחנה מדויקת ככל האפשר.
חשוב מאוד: השתמש בפרטים המספריים או בתוכן המשמעותי שהופיע בספר הרכב – כולל נתונים, הוראות, אזהרות וכל פרט רלוונטי.
ציין במפורש אם המידע מתוך הספר עזר או לא.

מידע טכני מתוך ספר הרכב:
{combinedContent}

בסיום, הצג אבחנה מבוססת, כולל:
- סיווג חומרה (קל / בינוני / חמור)
- הצעות לפתרון
- עלות משוערת אם ניתן להעריך.
""";



    

    var solution = await _aiService.GetCompletionAsync(finalPrompt);

    // הוספת fallback אם קיים
    if (!string.IsNullOrEmpty(fallbackMessage))
    {
        solution = $"⚠️ {fallbackMessage}\n\n" + solution;
    }

    // הוספת הקישורים לסוף התשובה
    if (manualLinks.Any())
    {
        solution += "\n\n📘 קישורים מתוך ספר הרכב: ";
        solution += string.Join(" | ", manualLinks.Select(entry =>
            $"{entry.Key}: {entry.Value?.Replace("\n", "").Replace("\r", "").Trim()}"));
    }

    // שמירה
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






public async Task<string> RunAdvancedDiagnosisAsync(
    string username,
    string email,
    string company,
    string model,
    int year,
    string problemDescription,
    Dictionary<string, string> followUpAnswers,
    List<string>? answers = null,
    string? licensePlate = null)
{
    if (!await _userRepository.IsValidUserAsync(username, email))
        return "⚠️ המשתמש לא נמצא במערכת.";

    // אם אין תשובות בכלל – שולחים שאלות
    if ((followUpAnswers == null || followUpAnswers.Count == 0) && (answers == null || answers.Count == 0))
    {
        var questions = await _aiService.GenerateFollowUpQuestionsAsync(problemDescription);

        if (questions != null && questions.Any())
        {
            var key = $"{username}|{email}|{company}|{model}|{year}|{problemDescription}";
            await _followUpStore.SaveQuestionsAsync(key, questions);

            var response = new
            {
                Message = "נדרשות הבהרות נוספות לאבחנה מדויקת. אנא מלא את השדות בתבנית הבאה ושלח שוב.",
                Questions = questions,
                Template = new
                {
                    username,
                    email,
                    licensePlate,
                    problemDescription,
                    answers = questions.Select(q => "").ToList()
                }
            };

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            return JsonSerializer.Serialize(response, jsonOptions);
        }
    }

    // ממפה תשובות פשוטות לפי שאלות שנשמרו
    if ((followUpAnswers == null || followUpAnswers.Count == 0) && answers?.Count > 0)
    {
        var key = $"{username}|{email}|{company}|{model}|{year}|{problemDescription}";
        var storedQuestions = await _followUpStore.GetQuestionsAsync(key);

        followUpAnswers = new Dictionary<string, string>();
        for (int i = 0; i < answers.Count && i < storedQuestions.Count; i++)
        {
            followUpAnswers[storedQuestions[i]] = answers[i];
        }
    }

    // שלב 1: תשובת GPT עם שאלות המשך
    var aiResult = await _aiService.RunAdvancedDiagnosisAsync(
        company, model, year, problemDescription, followUpAnswers
    );

    // שלב 2: שליפת נושא + מילות מפתח מתוך תיאור התקלה
    var topicData = await _problemTopicService.ExtractTopicAndKeywordsAsync(problemDescription);
    Console.WriteLine($"🔍 נושא שזוהה: {topicData.topic}");
    Console.WriteLine("📚 מילים נרדפות שזוהו:");
    foreach (var word in topicData.keywords)
        Console.WriteLine($"- {word}");

    // 🛠️ הוספת חומרה והערכת מחיר לטקסט
    if (!string.IsNullOrEmpty(aiResult.Severity))
    {
        aiResult.AIResponse += $"\n\n דרגת חומרה: {aiResult.Severity switch
        {
            "High" => "חמור – יש לפנות למוסך בהקדם.",
            "Medium" => "בינוני – לא דחוף אך מומלץ לבדוק.",
            "Low" => "קל – ניתן להמתין לטיפול הבא.",
            _ => aiResult.Severity
        }}";
    }

    if (!string.IsNullOrEmpty(aiResult.EstimatedCost))
    {
        aiResult.AIResponse += $"\n הערכת עלות לתיקון: {aiResult.EstimatedCost}";
    }

    // שלב 3: חיפוש קישורים
    var (manualLinks, fallbackMessage) = _manualLinkService.FindLinks(
        company, model, year, topicData.topic, topicData.keywords);

    if (!string.IsNullOrEmpty(fallbackMessage))
    {
        aiResult.AIResponse = $"⚠️ {fallbackMessage}\n\n" + aiResult.AIResponse;
    }

    if (manualLinks.Any())
    {
        aiResult.AIResponse += "\n\n📘 מידע נוסף מתוך ספר הרכב: ";
        aiResult.AIResponse += string.Join(" | ", manualLinks.Select(entry =>
            $"{entry.Key}: {entry.Value?.Replace("\n", "").Replace("\r", "").Trim()}"));
    }

    // שלב 4: שמירה לקובץ
    var carCall = new Car
    {
        Username = username,
        Email = email,
        Company = company,
        Model = model,
        Year = year,
        ProblemDescription = problemDescription,
        AIResponse = aiResult.AIResponse,
        LicensePlate = licensePlate,
        FollowUp = followUpAnswers,
        FollowUpQuestions = followUpAnswers?.Keys.ToList()
    };

    var existingCalls = await _carsCallsRepository.ReadCallsAsync();
    existingCalls.Add(carCall);
    await _carsCallsRepository.SaveCallsAsync(existingCalls);

    // שלב 5: תשובה למשתמש
    var result = new
    {
        Username = username,
        Email = email,
        Company = company,
        Model = model,
        Year = year,
        ProblemDescription = problemDescription,
        LicensePlate = licensePlate,
        FollowUp = followUpAnswers,
        AIResponse = aiResult.AIResponse,
        Severity = aiResult.Severity,
        EstimatedCost = aiResult.EstimatedCost,
        Links = manualLinks
    };

    var jsonResultOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    return JsonSerializer.Serialize(result, jsonResultOptions);
}



    }
}
