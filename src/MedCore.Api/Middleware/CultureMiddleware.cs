namespace MedCore.Api.Middleware;

using MedCore.Common.Localization;
using MedCore.Common.Services;
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
        ICultureResolver cultureResolver)
    {
        var culture = await ResolveAsync(context, cultureResolver);
        currentCultureService.SetCulture(culture);
        await _next(context);
    }
    
    private static async Task<string> ResolveAsync(
        HttpContext context,
        ICultureResolver cultureResolver)
    {
        if (context.User.Identity?.IsAuthenticated is not true)
            return ResolveFromHeader(context.Request.Headers.AcceptLanguage.ToString());
        
        var userIdClaim = context.User
            .FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
            return ResolveFromHeader(context.Request.Headers.AcceptLanguage.ToString());
        
        return await cultureResolver.ResolveForUserAsync(userId, context.RequestAborted);
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