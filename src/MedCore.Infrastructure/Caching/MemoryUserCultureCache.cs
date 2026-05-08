namespace MedCore.Infrastructure.Caching;

using MedCore.Common.Caching;
using Microsoft.Extensions.Caching.Memory;

internal sealed class MemoryUserCultureCache : IUserCultureCache
{
    private readonly IMemoryCache _cache;

    public MemoryUserCultureCache(IMemoryCache cache)
    {
        _cache = cache;
    }
    
    public bool TryGetCultureForUser(Guid userId, out string? culture)
    {
        return _cache.TryGetValue(CacheKeys.UserCulture(userId), out culture);
    }

    public void SetCultureForUser(Guid userId, string culture)
    {
        _cache.Set(CacheKeys.UserCulture(userId), culture, new MemoryCacheEntryOptions
        {
            SlidingExpiration = CacheKeys.UserCultureExpiry
        });
    }
}