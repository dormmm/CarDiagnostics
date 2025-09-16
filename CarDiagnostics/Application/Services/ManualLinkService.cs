using System.Diagnostics;                 // NEW
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using CarDiagnostics.Domain.Models.Interfaces; // IAzureStorageService
using CarDiagnostics.Services;

namespace CarDiagnostics.Services
{
    public class ManualLinkService
    {
        private readonly IAzureStorageService _storage;
        private readonly string _fileName;

        // === NEW ===
        private readonly ILinkFetcherService _linkFetcher;

        private readonly Dictionary<string, JsonObject> _manuals = new();
        private bool _loaded = false;
        private readonly SemaphoreSlim _loadLock = new(1, 1);

        // היה:
        // public ManualLinkService(IAzureStorageService storage, string fileName)
        // NEW: מוסיפים ILinkFetcherService
        public ManualLinkService(IAzureStorageService storage, string fileName, ILinkFetcherService linkFetcher)
        {
            _storage = storage;
            _fileName = fileName;
            _linkFetcher = linkFetcher;   // NEW
        }

        // טעינה עצלה של הקובץ מה-Blob (נקראת אוטומטית ב-FindLinks)
        private void EnsureLoaded()
        {
            if (_loaded) return;

            _loadLock.Wait();
            try
            {
                if (_loaded) return;

                Console.WriteLine($"☁️ טוען מדריכים מקובץ Azure בשם: {_fileName}");
                var json = _storage.DownloadFileAsync(_fileName).GetAwaiter().GetResult();

                if (string.IsNullOrEmpty(json))
                    throw new FileNotFoundException("❌ קובץ המדריכים לא נמצא בענן", _fileName);

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                _manuals.Clear();
                foreach (var make in root.EnumerateObject())
                {
                    try
                    {
                        var obj = JsonSerializer.Deserialize<JsonObject>(make.Value.GetRawText());
                        if (obj != null)
                        {
                            _manuals[make.Name] = obj;
                            Console.WriteLine($"✅ נטען יצרן: {make.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ שגיאה בטעינת יצרן '{make.Name}': {ex.Message}");
                    }
                }

                _loaded = true;
                Console.WriteLine($"📦 סה\"כ {_manuals.Count} יצרנים נטענו.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ שגיאה כללית בטעינת JSON: {ex.Message}");
                throw;
            }
            finally
            {
                _loadLock.Release();
            }
        }

        public (Dictionary<string, string> Links, string? FallbackMessage)
            FindLinks(string make, string model, int year, string topic, List<string> keywords)
        {
            EnsureLoaded();

            make = make.Trim().ToLowerInvariant();
            model = model.Trim().ToLowerInvariant();
            string yearStr = year.ToString();

            Console.WriteLine($"📁 קלט: make='{make}', model='{model}', year='{year}', topic='{topic}'");

            var results = new Dictionary<string, string>();
            string? fallbackMessage = null;

            Console.WriteLine("🧪 מפתחות קיימים בקובץ:");
            foreach (var k in _manuals.Keys)
                Console.WriteLine($"- {k} (normalized: {k.Trim().ToLowerInvariant()})");

            var actualMake = _manuals.Keys.FirstOrDefault(k => k.Trim().ToLowerInvariant() == make);
            if (actualMake == null)
            {
                Console.WriteLine($"❌ יצרן '{make}' לא נמצא בקובץ.");
                return (results, null);
            }

            var modelsObject = _manuals[actualMake].AsObject();

            var actualModel = modelsObject
                .Where(kvp => kvp.Key.Trim().ToLowerInvariant() == model)
                .Select(kvp => kvp.Key)
                .FirstOrDefault();

            if (actualModel == null)
            {
                Console.WriteLine($"❌ דגם '{model}' לא נמצא תחת היצרן '{actualMake}'");
                Console.WriteLine("📌 דגמים זמינים:");
                foreach (var kvp in modelsObject)
                    Console.WriteLine($"- {kvp.Key}");

                actualModel = modelsObject
                    .Select(kvp => kvp.Key)
                    .FirstOrDefault(k => k.ToLowerInvariant().Contains("series"))
                    ?? modelsObject.Select(kvp => kvp.Key).FirstOrDefault();

                if (actualModel != null)
                {
                    fallbackMessage = $"⚠️ הדגם '{model}' לא נמצא במערכת, אך מצאנו דגם קרוב: '{actualModel}'.";
                    Console.WriteLine(fallbackMessage);
                }
                else
                {
                    return (results, null);
                }
            }

            var yearsObject = modelsObject[actualModel].AsObject();

            var yearEntry = yearsObject.FirstOrDefault(entry =>
            {
                var rangeParts = entry.Key.Split('-');
                if (rangeParts.Length == 2 &&
                    int.TryParse(rangeParts[0], out var start) &&
                    int.TryParse(rangeParts[1], out var end))
                {
                    return year >= start && year <= end;
                }
                return entry.Key.Contains(yearStr);
            });

            if (yearEntry.Value is null)
            {
                fallbackMessage ??= $"⚠️ ";
                fallbackMessage += $"לא נמצא מידע מדויק לשנת {year}, אך מצאנו מידע משנת ";

                var closest = yearsObject
                    .Where(e => int.TryParse(e.Key.Split('-')[0], out _))
                    .OrderBy(e =>
                    {
                        var firstYear = int.Parse(e.Key.Split('-')[0]);
                        return Math.Abs(year - firstYear);
                    })
                    .FirstOrDefault();

                if (closest.Value is null)
                {
                    Console.WriteLine("❌ לא נמצא טווח שנה תואם כלל.");
                    return (results, fallbackMessage);
                }

                fallbackMessage += $"{closest.Key} שהיא השנה הקרובה ביותר.";
                yearEntry = closest;
            }

            Console.WriteLine($"✅ טווח שנה תואם: '{yearEntry.Key}'");

            var allCandidates = new List<(string key, string? link)>();
            var topicsRaw = yearEntry.Value;

            try
            {
                if (topicsRaw is JsonObject flatLinks &&
                    flatLinks.All(kv => kv.Value is JsonValue || kv.Value is JsonNode && ((JsonNode)kv.Value)?.GetValue<string>() != null))
                {
                    foreach (var pair in flatLinks)
                    {
                        var cleanedKey = pair.Key.Trim();
                        if (!allCandidates.Any(c => c.key.Equals(cleanedKey, StringComparison.OrdinalIgnoreCase)))
                            allCandidates.Add((cleanedKey, pair.Value?.ToString()?.Trim()));
                    }
                }
                else
                {
                    var topics = topicsRaw.AsObject();
                    foreach (var section in topics)
                    {
                        if (section.Value is JsonObject links)
                        {
                            foreach (var link in links)
                            {
                                var cleanedKey = link.Key.Trim();
                                if (!allCandidates.Any(c => c.key.Equals(cleanedKey, StringComparison.OrdinalIgnoreCase)))
                                    allCandidates.Add((cleanedKey, link.Value?.ToString()?.Trim()));
                            }
                        }
                        else if (section.Value is JsonValue val)
                        {
                            var cleanedKey = section.Key.Trim();
                            if (!allCandidates.Any(c => c.key.Equals(cleanedKey, StringComparison.OrdinalIgnoreCase)))
                                allCandidates.Add((cleanedKey, val.ToString().Trim()));
                        }
                    }
                }
            }
            catch (ArgumentException ex) when (ex.Message.Contains("already been added"))
            {
                Console.WriteLine("⚠️ זוהתה כפילות בקובץ JSON – חלק מהמידע לא נטען.");
                fallbackMessage ??= "⚠️ חלק מהמידע לא נטען עקב כפילות בקובץ.";
            }

            Console.WriteLine($"🔍 נבדקו {allCandidates.Count} קישורים קיימים.");

            var allKeywords = new[] { topic }
                .Concat(keywords)
                .Select(k => k.Trim().ToLowerInvariant())
                .Distinct()
                .ToList();

            Console.WriteLine("🔑 מילות מפתח לבדיקה:");
            foreach (var kw in allKeywords)
                Console.WriteLine($"- {kw}");

            // ניסיון ראשון – לפחות שתי מילות מפתח
            foreach (var (key, link) in allCandidates)
            {
                var loweredKey = key.ToLowerInvariant();
                int matchCount = allKeywords.Count(kw =>
                    loweredKey.Contains(kw) ||
                    kw.Contains(loweredKey) ||
                    loweredKey.TrimEnd('s') == kw.TrimEnd('s') ||
                    loweredKey.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries)
                        .Any(part => part == kw || part.TrimEnd('s') == kw.TrimEnd('s')));

                if (matchCount >= 2 && link != null && !results.ContainsKey(key))
                {
                    results[key] = link;
                }
            }

            // חיפוש מרוכך אם אין תוצאות
            if (!results.Any())
            {
                foreach (var keyword in allKeywords)
                {
                    foreach (var (key, link) in allCandidates)
                    {
                        var loweredKey = key.ToLowerInvariant();
                        bool isMatch =
                            loweredKey.Contains(keyword) ||
                            keyword.Contains(loweredKey) ||
                            loweredKey.TrimEnd('s') == keyword.TrimEnd('s') ||
                            loweredKey.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries)
                                .Any(part => part == keyword || part.TrimEnd('s') == keyword.TrimEnd('s'));

                        if (isMatch && link != null && !results.ContainsKey(key))
                        {
                            results[key] = link;
                        }
                    }
                }
            }

                       if (!results.Any())
                Console.WriteLine("❌ לא נמצא אף קישור.");

            // ===== Parallel enrichment (Concurrency) =====
            try
            {
                var urls = results.Values
                    .Where(u => !string.IsNullOrWhiteSpace(u))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (urls.Count > 0)
                {
                    var sw = Stopwatch.StartNew();
                    _ = _linkFetcher
                        .FetchTitlesAsync(urls, maxParallel: 5, ct: CancellationToken.None)
                        .GetAwaiter().GetResult();
                    sw.Stop();

                    Console.WriteLine($"[FindLinks] Parallel enrichment took {sw.ElapsedMilliseconds} ms for {urls.Count} links.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FindLinks] Parallel enrichment failed: {ex.Message}");
            }
            // ===== end =====

            return (results, fallbackMessage);
        }
    }
}

