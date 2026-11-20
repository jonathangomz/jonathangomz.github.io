using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Portfolio.Application.Abstractions;

namespace Portfolio.Infrastructure.Caching;

public sealed class MemoryCacheService(
    IMemoryCache memoryCache,
    ILogger<MemoryCacheService> logger) : ICacheService
{
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken ct = default)
    {
        if (memoryCache.TryGetValue(key, out T? cachedValue) && cachedValue is not null)
        {
            logger.LogInformation("Cache hit for {CacheKey} (read from cache)", key);
            return cachedValue;
        }

        logger.LogInformation("Cache miss for {CacheKey} (reading from repository)", key);
        var value = await factory(ct);

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration
        };

        memoryCache.Set(key, value, options);
        return value;
    }
}
