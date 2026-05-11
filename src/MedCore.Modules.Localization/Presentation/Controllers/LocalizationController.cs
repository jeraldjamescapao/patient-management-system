namespace MedCore.Modules.Localization.Presentation.Controllers;

using Asp.Versioning;
using MedCore.Common.Authorization;
using MedCore.Common.Controllers;
using MedCore.Common.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiVersion("1")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Roles = AppRoles.Admin)]
public sealed class LocalizationController : BaseApiController
{
    private readonly ILocalizerCache _localizerCache;

    public LocalizationController(ILocalizerCache localizerCache)
    {
        _localizerCache = localizerCache;
    }

    [HttpPost("translations/cache/refresh")]
    public async Task<IActionResult> RefreshTranslationCacheAsync(CancellationToken ct)
    {
        _localizerCache.InvalidateCache();
        await _localizerCache.LoadAsync(ct);
        return NoContent();
    }
}