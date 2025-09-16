using System.Collections.Concurrent;
using System.Net.Http;

namespace CarDiagnostics.Services;

public interface ILinkFetcherService
{
    Task<IReadOnlyList<(string Url, string? Title)>> FetchTitlesAsync(
        IEnumerable<string> urls, int maxParallel = 5, CancellationToken ct = default);
}

public class LinkFetcherService : ILinkFetcherService
{
    private readonly IHttpClientFactory _httpClientFactory;
    public LinkFetcherService(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

    public async Task<IReadOnlyList<(string Url, string? Title)>> FetchTitlesAsync(
        IEnumerable<string> urls, int maxParallel = 5, CancellationToken ct = default)
    {
        var http = _httpClientFactory.CreateClient();
        var results = new ConcurrentBag<(string Url, string? Title)>();
        using var gate = new SemaphoreSlim(maxParallel);

        var tasks = urls.Select(async url =>
        {
            await gate.WaitAsync(ct);
            try
            {
                using var resp = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
                string? title = null;

                if (resp.IsSuccessStatusCode)
                {
                    var html = await resp.Content.ReadAsStringAsync(ct);
                    var start = html.IndexOf("<title", StringComparison.OrdinalIgnoreCase);
                    if (start >= 0)
                    {
                        var gt = html.IndexOf('>', start);
                        var end = html.IndexOf("</title>", gt, StringComparison.OrdinalIgnoreCase);
                        if (gt > 0 && end > gt)
                            title = html.Substring(gt + 1, end - (gt + 1)).Trim();
                    }
                }

                results.Add((url, title));
            }
            catch
            {
                results.Add((url, null));
            }
            finally
            {
                gate.Release();
            }
        });

        await Task.WhenAll(tasks);
        return results.ToList();
    }
}
