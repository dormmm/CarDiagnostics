using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CarDiagnostics.Services
{
    public class ProblemTopicService
    {
        private readonly HttpClient _httpClient;
        private readonly string _openAiKey;

        public ProblemTopicService(string openAiKey)
        {
            _httpClient = new HttpClient();
            _openAiKey = openAiKey;
        }

       public async Task<(string topic, List<string> keywords)> ExtractTopicAndKeywordsAsync(string problemDescription)
{
   var prompt = @$"
You are a smart car assistant that receives a **problem description** written in any language, including Hebrew.

Your task is to:
1. Detect the **main part** of the car that the problem is about (e.g. screen, battery, engine).
2. Return it in the field 'topic' in English only.
3. Also return a list of related **keywords** and synonyms (in English only), including terms users might search in a car manual.

Only output a JSON object like this:
{{ 
  ""topic"": ""<main part in English>"",
  ""keywords"": [""keyword1"", ""keyword2"", ...]
}}

Be concise. Do not explain.

Problem: {problemDescription}
";


    var requestBody = new
    {
        model = "gpt-3.5-turbo",
        messages = new[]
        {
            new { role = "user", content = prompt }
        },
        temperature = 0.3
    };

    var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
    {
        Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
    };

    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _openAiKey);

    var response = await _httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();

    var json = await response.Content.ReadAsStringAsync();
    var result = JsonDocument.Parse(json);

    var content = result.RootElement
        .GetProperty("choices")[0]
        .GetProperty("message")
        .GetProperty("content")
        .GetString();

    var parsed = JsonDocument.Parse(content!);

    var topic = parsed.RootElement.GetProperty("topic").GetString() ?? "";
    var keywords = parsed.RootElement.GetProperty("keywords")
        .EnumerateArray()
        .Select(k => k.GetString() ?? "")
        .ToList();

    return (topic, keywords);
}

    }
}
