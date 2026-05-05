namespace MedCore.Modules.Identity.Presentation.Controllers;

using Asp.Versioning;
using MedCore.Common.Controllers;
using MedCore.Common.Localization;
using MedCore.Modules.Identity.Domain.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiVersion("1")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Roles = IdentityRoles.Admin)]
public sealed class AdminController : BaseApiController
{
    private readonly ILocalizerCache _localizerCache;

    public AdminController(ILocalizerCache localizerCache)
    {
        _localizerCache = localizerCache;
    }
    
    [HttpPost("translations/refresh")]
    public async Task<IActionResult> RefreshTranslationsAsync(CancellationToken ct)
    {
        _localizerCache.InvalidateCache();
        await _localizerCache.LoadAsync(ct);
        return NoContent();
    }
}