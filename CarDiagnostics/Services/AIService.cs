using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CarDiagnostics.Services
{
    public class AIService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public AIService(IConfiguration configuration)
        {
            // ✅ בדיקה אם ה-API Key חסר, אחרת זריקת שגיאה
            _apiKey = configuration["OpenAI:ApiKey"] 
                      ?? throw new Exception("API Key is missing! Please check your appsettings.json.");

            _httpClient = new HttpClient();
        }

        public async Task<string> GetDiagnosisAsync(string company, string model, int year, string problemDescription)
{
    try
    {
        var requestBody = new
        {
            model = "gpt-3.5-turbo", // וודא שאתה משתמש במודל הנתמך
            messages = new[]
           {
        new { role = "system", content = @"
            אתה מומחה למכוניות. תן אבחנה מקצועית לבעיות רכב לפי סוג הרכב, הדגם והשנה.
                        ציין את סיווג החומרה של הבעיה על פי הקטגוריות הבאות:
            - קל: ניתן לתקן בטיפול הבא.
            - בינוני: לא מסכן, אבל כדאי לתקן בהקדם.
            - חמור: צריך לנסוע למוסך מיידית.
            - סכנה: אין לנסוע כלל, חובה להזמין גרר.

            בנוסף, תן הערכת מחיר ממוצעת לתיקון בהתחשב בסוג הרכב.
        "},
        new { role = "user", content = $@"
            הרכב הוא {company} {model} משנת {year}. 
            הבעיה היא: {problemDescription}.

            תן אבחנה טכנית, סיווג חומרה והערכת מחיר.
        "}
    },
            temperature = 0.4,
            max_tokens = 400 // ✅ מגביל את כמות המילים בתשובה
        };

        var jsonRequest = JsonSerializer.Serialize(requestBody);
        var requestContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", requestContent);
        var responseString = await response.Content.ReadAsStringAsync();

        Console.WriteLine("AI Response: " + responseString);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"AI API Error: {response.StatusCode}");
            return $"שגיאה בתקשורת עם ה-AI: {response.StatusCode}";
        }

        using var doc = JsonDocument.Parse(responseString);

        if (doc.RootElement.TryGetProperty("choices", out JsonElement choices) && choices.GetArrayLength() > 0)
        {
            string answer = choices[0].GetProperty("message").GetProperty("content").GetString() ?? "לא התקבלה תשובה.";

            // ✅ מחיקת מעברי שורה מהתשובה
            return answer.Replace("\n", " ");
        }

        return "לא התקבלה תשובה מה-AI.";
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error communicating with AI: {ex.Message}");
        return "שגיאה בעת שליחת הבקשה ל-AI.";
    }
}

    }
}