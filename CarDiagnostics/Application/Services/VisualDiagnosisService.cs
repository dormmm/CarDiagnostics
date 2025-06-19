using System.Text.Json;
using System.Net.Http.Headers;
using System.Text;
using CarDiagnostics.Services;

namespace CarDiagnostics.Services
{
    public class VisualDiagnosisService
    {
        private readonly string _apiKey;
        private readonly LicensePlateService _licensePlateService;
        private readonly ManualLinkService _manualLinkService;

        public VisualDiagnosisService(string apiKey, LicensePlateService licensePlateService, ManualLinkService manualLinkService)
        {
            _apiKey = apiKey;
            _licensePlateService = licensePlateService;
            _manualLinkService = manualLinkService;
        }

        public async Task<object> AnalyzeAsync(string base64Image, string description, string licensePlate)
        {
            var carInfo = _licensePlateService.GetCarByPlate(licensePlate);
            if (carInfo == null)
                return new { error = "לא נמצאו פרטי רכב לפי מספר הרישוי" };

            var company = carInfo["manufacturer"]?.ToString() ?? "";
            var model = carInfo["model"]?.ToString() ?? "";
            var yearStr = carInfo["year"]?.ToString() ?? "";
            int.TryParse(yearStr, out var year);

            var prompt = $"תאר את התקלה לפי התמונה והתיאור: \"{description}\". הרכב הוא {company} {model} {yearStr}. התייחס לרכב הזה בלבד.";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var requestBody = new
            {
                model = "gpt-4o",
                messages = new object[]
                {
                    new
                    {
                        role = "system",
                        content = "אתה מכונאי רכב מקצועי. תפקידך הוא לנתח תמונות של תקלות ברכב לפי התיאור והתמונה שנשלחו, ולהסביר למשתמש מה עשויה להיות הבעיה ומה כדאי לבדוק."
                    },
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = prompt },
                            new { type = "image_url", image_url = new { url = $"data:image/png;base64,{base64Image}" } }
                        }
                    }
                }
            };

            var response = await client.PostAsync("https://api.openai.com/v1/chat/completions",
                new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json"));

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(content);
            var gptText = result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            var (links, fallback) = _manualLinkService.FindLinks(company, model, year, description, ExtractKeywords(gptText ?? ""));

            return new
            {
                carInfo = new { Company = company, Model = model, Year = year },
                gptResponse = gptText,
                relevantLinks = links,
                fallbackMessage = fallback
            };
        }

        private List<string> ExtractKeywords(string text)
        {
            return text
                .Split(new[] { ' ', '.', ',', ':', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(word => word.Length > 2)
                .Distinct()
                .ToList();
        }
    }
}
