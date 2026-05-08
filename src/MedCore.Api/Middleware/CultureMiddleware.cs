namespace MedCore.Api.Middleware;

using MedCore.Common.Caching;
using MedCore.Common.Localization;
using MedCore.Common.Services;
using MedCore.Modules.Identity.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using System.IdentityModel.Tokens.Jwt;

internal sealed class CultureMiddleware
{
    private readonly RequestDelegate _next;

    public CultureMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(
        HttpContext context,
        ICurrentCultureService currentCultureService,
        UserManager<ApplicationUser> userManager,
        IMemoryCache cache,
        IUserCultureCache userCultureCache)
    {
        var culture = await ResolveAsync(context, userManager, cache, userCultureCache);
        currentCultureService.SetCulture(culture);
        await _next(context);
    }
    
    private static async Task<string> ResolveAsync(
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        IMemoryCache cache,
        IUserCultureCache userCultureCache)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return ResolveFromHeader(context.Request.Headers.AcceptLanguage.ToString());
        
        var userIdClaim = context.User
            .FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
            return ResolveFromHeader(context.Request.Headers.AcceptLanguage.ToString());
        
        var cacheKey = CacheKeys.UserCulture(userId);

        if (cache.TryGetValue(cacheKey, out string? cached) && cached is not null)
            return cached;

        // NOTE: Cache miss triggers a DB query via UserManager.
        // This happens on the first request, and immediately after UpdateCultureAsync sets
        // a fresh cache entry. In practice, the cache is warm for 30 minutes after any
        // culture update, so this path is rare. If traffic grows significantly,
        // the cache will be pre-warmed at login time.
        var user = await userManager.FindByIdAsync(userId.ToString());
        var resolved = ResolveFromPreference(user?.PreferredCulture);

        userCultureCache.SetCultureForUser(userId, resolved);

        return resolved;
    }
    
    private static string ResolveFromPreference(string? preferredCulture)
    {
        if (preferredCulture is null)
            return SupportedCultures.Default;

        return SupportedCultures.All.Contains(preferredCulture)
            ? preferredCulture
            : SupportedCultures.Default;
    }
    
    private static string ResolveFromHeader(string acceptLanguage)
    {
        if (string.IsNullOrWhiteSpace(acceptLanguage))
            return SupportedCultures.Default;

        // "fr-CH,fr;q=0.9,en;q=0.8" → take the first tag before any quality weight
        var primary = acceptLanguage
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(tag => tag.Split(';')[0].Trim())
            .FirstOrDefault();

        if (primary is null)
            return SupportedCultures.Default;

        return SupportedCultures.All.Contains(primary)
            ? primary
            : SupportedCultures.Default;
    }
}