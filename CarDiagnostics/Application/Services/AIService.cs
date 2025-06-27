using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using CarDiagnostics.Models;
using System.Net.Http.Headers;




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
                    max_tokens = 800
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

        public async Task<AIResult> RunAdvancedDiagnosisAsync(
      string company,
      string model,
      int year,
      string problemDescription,
      Dictionary<string, string> followUpAnswers)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                var followUpText = string.Join("\n", followUpAnswers.Select(x => $"- {x.Key}: {x.Value}"));

                // ...existing code...
                var prompt = $@"
המשתמש דיווח על תקלה ברכב {company} {model} {year}.
תיאור התקלה: {problemDescription}
תשובות המשתמש לשאלות המשך:
{followUpText}

⚠️ חשוב מאוד: התשובה שלך חייבת להתבסס על הדגם הספציפי של הרכב (כולל שנת הייצור).  
אל תיתן אבחנה כללית שמתאימה לכל רכב – תתייחס לתקלות אופייניות בדגם הזה בלבד.

אנא בצע את הפעולות הבאות:

1. ספק אבחנה טכנית מדויקת ככל האפשר – הסבר מה עלול לגרום לתקלה, ומה הסיכון אם היא לא תטופל.
2. ציין בצורה ברורה אילו חלקים חשודים כתקולים (לדוגמה: מדחס מזגן, חיישן טמפרטורה, פיוז, ECU).
3. ציין את דרגת החומרה של הבעיה (קל / בינוני / חמור / סכנה).
4. חובה לכלול גם הערכת מחיר ממוצעת לתיקון – מספר מדויק או טווח (למשל: 800–1200 ש""ח).

ולאחר מכן כלול גם את הסעיפים הבאים:

5. פעולות בדיקה עצמית שהמשתמש יכול לבצע לבד בבית.
6. משך זמן טיפול משוער (במוסך רגיל).
7. השלכות אפשריות אם התקלה לא תטופל.
8. סימנים נוספים שיכולים להעיד על התקלה.
9. הסבר פשוט על איך פועלת המערכת התקולה.
10. האם התקלה עלולה לגרום לכישלון בטסט (מבחן רישוי שנתי בישראל).

- שלב לפחות ציטוט אחד **מתוך הטקסט באנגלית**, תרגם אותו לעברית, וכתוב גם את המשפט המקורי באנגלית בסוגריים.
- אל תסתפק בהתייחסות כללית בלבד – חובה לכלול משפט ישיר מתוך המידע שהועבר.


אם יש מידע מתוך ספר הרכב (מצורף בסוף), **שלב אותו בתוך האבחנה שלך**. 
ציין אם הוא רלוונטי או לא. אם רלוונטי – תרגם אותו לעברית ושתול אותו כחלק מההסבר.
אם אין קשר – כתוב זאת במפורש.

📄 מידע מתוך ספר הרכב:
";

                // ...existing code...

                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                new {
                    role = "system",
                    content = "אתה מומחה לרכב. עבודתך היא לאבחן תקלות בצורה מדויקת ומקצועית לפי תיאור הבעיה והתשובות לשאלות המשך. כל תשובה חייבת לכלול חלקים חשודים, חומרה ועלות."
                },
                new {
                    role = "user",
                    content = prompt
                }
            },
                    temperature = 0.4,
                    max_tokens = 800
                };

                var jsonRequest = JsonSerializer.Serialize(requestBody);
                var requestContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

                var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", requestContent);
                var responseString = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("AI Response (advanced): {Response}", responseString);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("AI API Error: {StatusCode}", response.StatusCode);
                    return new AIResult { AIResponse = $"שגיאה בתקשורת עם ה-AI: {response.StatusCode}" };
                }

                using var doc = JsonDocument.Parse(responseString);

                if (doc.RootElement.TryGetProperty("choices", out JsonElement choices) && choices.GetArrayLength() > 0)
                {
                    string answer = choices[0].GetProperty("message").GetProperty("content").GetString() ?? "לא התקבלה תשובה.";
                    answer = answer.Replace("\n", " ");

                    return new AIResult
                    {
                        AIResponse = answer,
                        Severity = ExtractSeverity(answer),
                        EstimatedCost = ExtractEstimatedCost(answer),
                        Links = ExtractLinks(answer)
                    };
                }

                return new AIResult { AIResponse = "לא התקבלה תשובה מה-AI." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RunAdvancedDiagnosisAsync.");
                return new AIResult { AIResponse = "שגיאה בעת שליחת הבקשה ל-AI." };
            }
        }


        private string? ExtractSeverity(string text)
        {
            if (text.Contains("חמור", StringComparison.OrdinalIgnoreCase)) return "High";
            if (text.Contains("בינוני", StringComparison.OrdinalIgnoreCase)) return "Medium";
            if (text.Contains("קל", StringComparison.OrdinalIgnoreCase)) return "Low";
            return null;
        }

        private string? ExtractEstimatedCost(string text)
        {
            var match = Regex.Match(text, @"\d{2,5}\s*ש""?ח");
            return match.Success ? match.Value : null;
        }

        private List<string>? ExtractLinks(string text)
        {
            return text.Contains("https://")
                ? Regex.Matches(text, @"https?://\S+")
                      .Select(m => m.Value)
                      .Distinct()
                      .ToList()
                : null;
        }

        public async Task<List<string>> GenerateFollowUpQuestionsAsync(string problemDescription)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                var prompt = $@"
בהתבסס על הבעיה הבאה שתיאר משתמש ברכב: ""{problemDescription}""
הצע שלוש שאלות המשך שיכולות לעזור לדייק את האבחנה. השב בפורמט של רשימת שאלות בלבד בלי הסברים.";


                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new {
                            role = "system",
                            content = "אתה יועץ טכני לרכב. עבודתך היא לשאול שאלות המשך ממוקדות שיעזרו לדייק את התקלה."
                        },
                        new {
                            role = "user",
                            content = prompt
                        }
                    },
                    temperature = 0.3,
                    max_tokens = 200
                };

                var jsonRequest = JsonSerializer.Serialize(requestBody);
                var requestContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

                var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", requestContent);
                var responseString = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Follow-up question response: {Response}", responseString);

                using var doc = JsonDocument.Parse(responseString);
                if (doc.RootElement.TryGetProperty("choices", out JsonElement choices) && choices.GetArrayLength() > 0)
                {
                    string rawText = choices[0].GetProperty("message").GetProperty("content").GetString() ?? "";
                    var lines = rawText.Split('\n')
                        .Select(l => l.Trim())
                        .Where(l => !string.IsNullOrWhiteSpace(l))
                        .Select(l => l.TrimStart('-', '*', '1', '2', '3', '.', ')'))
                        .ToList();

                    return lines;
                }

                return new List<string> { "⚠️ לא התקבלו שאלות המשך מה-AI." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating follow-up questions.");
                return new List<string> { "⚠️ שגיאה בשליפת שאלות המשך." };
            }
        }
        
        public async Task<string> AnalyzeImageWithDescription(string base64Image, string description)
{
    var request = new
    {
        model = "gpt-4o",
        messages = new object[]
        {
            new {
                role = "user",
                content = new object[]
                {
                    new { type = "text", text = $"תאר לי את התקלה לפי התמונה הבאה והתיאור: {description}" },
                    new { type = "image_url", image_url = new {
                        url = $"data:image/png;base64,{base64Image}"
                    }}
                }
            }
        }
    };

    using var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    var response = await client.PostAsync("https://api.openai.com/v1/chat/completions",
        new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

    var content = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<JsonElement>(content);
    return result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
}

public async Task<string> GetCompletionAsync(string prompt)
{
    try
    {
        var client = _httpClientFactory.CreateClient();

        var requestBody = new
        {
            model = "gpt-3.5-turbo",
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.5,
            max_tokens = 800
        };

        var jsonRequest = JsonSerializer.Serialize(requestBody);
        var requestContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", requestContent);
        var responseString = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("AI GetCompletion Response: {Response}", responseString);

        if (!response.IsSuccessStatusCode)
            return $"שגיאה בתקשורת עם GPT: {response.StatusCode}";

        using var doc = JsonDocument.Parse(responseString);

        if (doc.RootElement.TryGetProperty("choices", out JsonElement choices) && choices.GetArrayLength() > 0)
        {
            return choices[0].GetProperty("message").GetProperty("content").GetString() ?? "לא התקבלה תשובה.";
        }

        return "לא התקבלה תשובה מה-GPT.";
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "שגיאה בביצוע GetCompletionAsync");
        return "שגיאה בבקשה ל-GPT.";
    }
}


    }
}
