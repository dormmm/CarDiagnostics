// File: Infrastructure/Caching/RedisCacheService.cs
namespace CarDiagnostics.Infrastructure.Caching;

using StackExchange.Redis;
using System.Text.Json;

public class RedisCacheService
{
    private readonly IDatabase _db;
    public RedisCacheService(IConnectionMultiplexer mux) => _db = mux.GetDatabase();

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
        => await _db.StringSetAsync(key, JsonSerializer.Serialize(value), ttl);

    public async Task<T?> GetAsync<T>(string key)
    {
        var raw = await _db.StringGetAsync(key);
        return raw.HasValue ? JsonSerializer.Deserialize<T>(raw!) : default;
    }

    public Task<bool> RemoveAsync(string key) => _db.KeyDeleteAsync(key);
}
