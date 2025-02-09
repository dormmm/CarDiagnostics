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
            // ג… ׳‘׳“׳™׳§׳” ׳׳ ׳”-API Key ׳—׳¡׳¨, ׳׳—׳¨׳× ׳–׳¨׳™׳§׳× ׳©׳’׳™׳׳”
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
            model = "gpt-3.5-turbo", // ׳•׳•׳“׳ ׳©׳׳×׳” ׳׳©׳×׳׳© ׳‘׳׳•׳“׳ ׳”׳ ׳×׳׳
            messages = new[]
            {
                new { role = "system", content = "׳׳×׳” ׳׳•׳׳—׳” ׳¨׳›׳‘. ׳¢׳ ׳” ׳×׳©׳•׳‘׳•׳× ׳§׳¦׳¨׳•׳× ׳•׳‘׳¨׳•׳¨׳•׳×." },
                new { role = "user", content = $"׳”׳¨׳›׳‘ ׳”׳•׳ {company} {model} ׳׳©׳ ׳× {year}. ׳”׳‘׳¢׳™׳” ׳”׳™׳: {problemDescription}. ׳׳™׳ ׳׳₪׳©׳¨ ׳׳×׳§׳ ׳–׳׳×?" }
            },
            temperature = 0.7,
            max_tokens = 200 // ג… ׳׳’׳‘׳™׳ ׳׳× ׳›׳׳•׳× ׳”׳׳™׳׳™׳ ׳‘׳×׳©׳•׳‘׳”
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
            return $"׳©׳’׳™׳׳” ׳‘׳×׳§׳©׳•׳¨׳× ׳¢׳ ׳”-AI: {response.StatusCode}";
        }

        using var doc = JsonDocument.Parse(responseString);

        if (doc.RootElement.TryGetProperty("choices", out JsonElement choices) && choices.GetArrayLength() > 0)
        {
            string answer = choices[0].GetProperty("message").GetProperty("content").GetString() ?? "׳׳ ׳”׳×׳§׳‘׳׳” ׳×׳©׳•׳‘׳”.";

            // ג… ׳׳—׳™׳§׳× ׳׳¢׳‘׳¨׳™ ׳©׳•׳¨׳” ׳׳”׳×׳©׳•׳‘׳”
            return answer.Replace("\n", " ");
        }

        return "׳׳ ׳”׳×׳§׳‘׳׳” ׳×׳©׳•׳‘׳” ׳׳”-AI.";
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error communicating with AI: {ex.Message}");
        return "׳©׳’׳™׳׳” ׳‘׳¢׳× ׳©׳׳™׳—׳× ׳”׳‘׳§׳©׳” ׳-AI.";
    }
}

    }
}