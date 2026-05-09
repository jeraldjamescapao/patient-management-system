namespace MedCore.Modules.Identity.Infrastructure.Services;

using MedCore.Common.Caching;
using MedCore.Common.Localization;
using MedCore.Common.Services;
using MedCore.Modules.Identity.Domain.Users;
using Microsoft.AspNetCore.Identity;

internal sealed class IdentityCultureResolver : ICultureResolver
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserCultureCache _userCultureCache;

    public IdentityCultureResolver(
        UserManager<ApplicationUser> userManager,
        IUserCultureCache userCultureCache)
    {
        _userManager = userManager;
        _userCultureCache = userCultureCache;
    }

    public async Task<string> ResolveForUserAsync(Guid userId, CancellationToken ct = default)
    {
        if (_userCultureCache.TryGetCultureForUser(userId, out var cached) && cached is not null)
            return cached;
        
        // Cache miss — query the database.
        // This only happens when the sliding window expires (30 minutes of inactivity).
        // Login and culture updates keep the cache warm in normal usage.
        var user = await _userManager.FindByIdAsync(userId.ToString());
        var culture = user?.PreferredCulture is not null &&
                      SupportedCultures.All.Contains(user.PreferredCulture)
            ? user.PreferredCulture
            : SupportedCultures.Default;

        _userCultureCache.SetCultureForUser(userId, culture);

        return culture;
    }
}