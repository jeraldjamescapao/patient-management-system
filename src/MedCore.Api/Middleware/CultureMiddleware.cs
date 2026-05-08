namespace MedCore.Api.Middleware;

using MedCore.Common.Caching;
using MedCore.Common.Localization;
using MedCore.Common.Services;
using MedCore.Modules.Identity.Domain.Users;
using Microsoft.AspNetCore.Identity;
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
        IUserCultureCache userCultureCache)
    {
        var culture = await ResolveAsync(context, userManager, userCultureCache);
        currentCultureService.SetCulture(culture);
        await _next(context);
    }
    
    private static async Task<string> ResolveAsync(
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        IUserCultureCache userCultureCache)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return ResolveFromHeader(context.Request.Headers.AcceptLanguage.ToString());
        
        var userIdClaim = context.User
            .FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
            return ResolveFromHeader(context.Request.Headers.AcceptLanguage.ToString());
        
        var cacheKey = CacheKeys.UserCulture(userId);

        if (userCultureCache.TryGetCultureForUser(userId, out var cached) && cached is not null)
            return cached;

        // NOTE: Cache miss triggers a DB query via UserManager.
        // This only occurs when the sliding window expires (30 minutes of inactivity).
        // Login pre-populates the cache, and culture updates keep it warm via SetCultureForUser.
        // This path is rare in normal usage.
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