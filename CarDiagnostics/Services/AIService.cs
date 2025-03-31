using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AIService> _logger;

        public AIService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AIService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            _apiKey = configuration["OpenAI:ApiKey"]
                      ?? throw new Exception("API Key is missing! Please check your appsettings.json.");
        }

        public async Task<string> GetDiagnosisAsync(string company, string model, int year, string problemDescription)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new {
                            role = "system",
                            content = @"
                                אתה מומחה למכוניות. תן אבחנה מקצועית לבעיות רכב לפי סוג הרכב, הדגם והשנה.
                                ציין את סיווג החומרה של הבעיה על פי הקטגוריות הבאות:
                                - קל: ניתן לתקן בטיפול הבא.
                                - בינוני: לא מסכן, אבל כדאי לתקן בהקדם.
                                - חמור: צריך לנסוע למוסך מיידית.
                                - סכנה: אין לנסוע כלל, חובה להזמין גרר.
                                בנוסף, תן הערכת מחיר ממוצעת לתיקון בהתחשב בסוג הרכב.
                            "
                        },
                        new {
                            role = "user",
                            content = $@"
                                הרכב הוא {company} {model} משנת {year}. 
                                הבעיה היא: {problemDescription}.
                                תן אבחנה טכנית, סיווג חומרה והערכת מחיר.
                            "
                        }
                    },
                    temperature = 0.4,
                    max_tokens = 400
                };

                var jsonRequest = JsonSerializer.Serialize(requestBody);
                var requestContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

                var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", requestContent);
                var responseString = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("AI Response: {Response}", responseString);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("AI API Error: {StatusCode}", response.StatusCode);
                    return $"שגיאה בתקשורת עם ה-AI: {response.StatusCode}";
                }

                using var doc = JsonDocument.Parse(responseString);

                if (doc.RootElement.TryGetProperty("choices", out JsonElement choices) && choices.GetArrayLength() > 0)
                {
                    string answer = choices[0].GetProperty("message").GetProperty("content").GetString() ?? "לא התקבלה תשובה.";
                    return answer.Replace("\n", " ");
                }

                return "לא התקבלה תשובה מה-AI.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error communicating with AI.");
                return "שגיאה בעת שליחת הבקשה ל-AI.";
            }
        }
    }
}
