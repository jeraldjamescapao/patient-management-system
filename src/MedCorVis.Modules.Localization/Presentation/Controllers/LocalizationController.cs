namespace MedCorVis.Modules.Localization.Presentation.Controllers;

using Asp.Versioning;
using MedCorVis.Common.Authorization;
using MedCorVis.Common.Controllers;
using MedCorVis.Modules.Localization.Application.Abstractions;
using MedCorVis.Modules.Localization.Application.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[ApiVersion("1")]
[Route("api/v{version:apiVersion}/translations")]
public sealed class LocalizationController : BaseApiController
{
    private readonly ITranslationService _translationService;

    public LocalizationController(ITranslationService translationService)
    {
        _translationService = translationService;   
    }

    [HttpPost("cache/refresh")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> RefreshTranslationCacheAsync(CancellationToken ct)
    {
        await _translationService.RefreshCacheAsync(ct);
        return NoContent();
    }
    
    [HttpGet]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> GetAllAsync(
        [FromQuery] string? culture, CancellationToken ct)
    {
        var result = await _translationService.GetAllAsync(culture, ct);
        return ToActionResult(result);
    }
    
    [HttpGet("{id:long}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> GetByIdAsync(long id, CancellationToken ct)
    {
        var result = await _translationService.GetByIdAsync(id, ct);
        return ToActionResult(result);
    }
    
    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateTranslationRequest request, CancellationToken ct)
    {
        var result = await _translationService.CreateAsync(request, ct);
        if (result.IsFailure) return ToActionResult(result);

        return ToActionResult(result, StatusCodes.Status201Created);
    }
    
    [HttpPut("{id:long}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> UpdateAsync(
        long id, [FromBody] UpdateTranslationRequest request, CancellationToken ct)
    {
        var result = await _translationService.UpdateAsync(id, request, ct);
        return ToActionResult(result);
    }
    
    [HttpDelete("{id:long}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> DeleteAsync(long id, CancellationToken ct)
    {
        var result = await _translationService.DeleteAsync(id, ct);
        if (result.IsFailure) return ToActionResult(result);

        return NoContent();
    }
}