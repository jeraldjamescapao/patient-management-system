namespace MedCorVis.Modules.Localization.Infrastructure.Services;

using MedCorVis.Common.Caching;
using MedCorVis.Common.Localization;
using MedCorVis.Modules.Localization.Application.Abstractions;
using MedCorVis.Modules.Localization.Application.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal sealed class DbMessageLocalizer : IMessageLocalizer, ILocalizerCache
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<DbMessageLocalizer> _logger;
    
    public DbMessageLocalizer(
        IServiceScopeFactory scopeFactory,
        IMemoryCache cache,
        ILogger<DbMessageLocalizer> logger)
    {
        _scopeFactory = scopeFactory;
        _cache = cache;
        _logger = logger;
    }
    
    public string Get(string key, string culture)
    {
        var translations = _cache
            .Get<IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>>(
                CacheKeys.Translations);

        if (translations is null)
        {
            TranslationLogMessages.TranslationCacheEmpty(_logger, key, culture, null);
            return key;
        }

        foreach (var candidate in BuildFallbackChain(culture))
        {
            if (translations.TryGetValue(candidate, out var dict) &&
                dict.TryGetValue(key, out var value))
                return value;
        }

        TranslationLogMessages.TranslationMissing(_logger, key, culture, null);

        return key;
    }
    
    public async Task LoadAsync(CancellationToken ct = default)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var repository = scope.ServiceProvider
            .GetRequiredService<ITranslationRepository>();

        var grouped = await repository.GetAllGroupedAsync(ct);
        
        _cache.Set(CacheKeys.Translations, grouped);

        TranslationLogMessages.TranslationCacheLoaded(_logger, grouped.Count, null);
    }
    
    public void InvalidateCache()
    {
        _cache.Remove(CacheKeys.Translations);
        TranslationLogMessages.TranslationCacheInvalidated(_logger, null);
    }
    
    private static IEnumerable<string> BuildFallbackChain(string culture)
    {
        yield return culture;

        var dashIndex = culture.IndexOf('-');
        if (dashIndex > 0)
            yield return culture[..dashIndex];

        if (culture != SupportedCultures.Default)
            yield return SupportedCultures.Default;
    }
}