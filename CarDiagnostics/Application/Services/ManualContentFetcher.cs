using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CarDiagnostics.Services
{
    public class ManualContentFetcher
    {
        private readonly HttpClient _httpClient = new();

        public async Task<string> FetchCleanContentAsync(string url)
        {
            Console.WriteLine($"🔍 התחלת שליפה מהקישור: {url}");

            try
            {
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                var html = await _httpClient.GetStringAsync(url);
                Console.WriteLine($"✅ התקבל HTML באורך: {html.Length}");

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var body = doc.DocumentNode.SelectSingleNode("//body");
                if (body == null)
                {
                    Console.WriteLine("⚠️ לא נמצאה תגית <body>");
                    return "[לא נמצא תוכן בגוף העמוד]";
                }

                var fullText = body.InnerText.Trim();

                // ✅ סינון לפי התחלה וסיום במספרים
                var lines = fullText.Split('\n');
                var sb = new StringBuilder();
                var buffer = new List<string>();
                bool insideBlock = false;
                var pageNumPattern = new Regex(@"^\d+(-\d+)?$");

                foreach (var rawLine in lines)
                {
                    var line = rawLine.Trim();

                    if (pageNumPattern.IsMatch(line))
                    {
                        if (!insideBlock)
                        {
                            // התחלה של קטע
                            buffer.Clear();
                            buffer.Add(line);
                            insideBlock = true;
                        }
                        else
                        {
                            // סיום קטע
                            buffer.Add(line);
                            sb.AppendLine(string.Join('\n', buffer));
                            sb.AppendLine(); // רווח בין קטעים
                            insideBlock = false;
                        }
                    }
                    else if (insideBlock)
                    {
                        buffer.Add(line);
                    }
                }

                var result = sb.ToString().Trim();

                if (string.IsNullOrWhiteSpace(result))
                {
                    Console.WriteLine("⚠️ לא נמצאו קטעים עם סימוני עמודים");
                    return "[לא נמצאו קטעים עם סימוני עמודים]";
                }

                // ✅ הדפסת *רק* הטקסט הרלוונטי ללוג
                Console.WriteLine("📄 טקסט מסונן שנשלח ל-GPT:\n");
                Console.WriteLine(result);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ שגיאה בשליפת תוכן מהקישור: {ex.Message}");
                return "❌ שגיאה בשליפת מידע מהקישור.";
            }
        }
    }
}
