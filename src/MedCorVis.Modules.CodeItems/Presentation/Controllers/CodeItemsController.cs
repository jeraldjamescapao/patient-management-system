namespace MedCorVis.Modules.CodeItems.Presentation.Controllers;

using Asp.Versioning;
using MedCorVis.Common.Authorization;
using MedCorVis.Common.Controllers;
using MedCorVis.Modules.CodeItems.Application.Abstractions;
using MedCorVis.Modules.CodeItems.Application.Contracts.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = AppRoles.Admin)]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/code-items")]
public sealed class CodeItemsController : BaseApiController
{
    private readonly ICodeItemService _service;

    public CodeItemsController(ICodeItemService service)
    {
        _service = service;
    }
    
    #region Categories
    
    [HttpGet("categories")]
    public async Task<IActionResult> GetAllCategoriesAsync(CancellationToken ct)
    {
        var result = await _service.GetAllCategoriesAsync(ct);
        return ToActionResult(result);
    }
    
    [HttpGet("categories/{id:long}")]
    public async Task<IActionResult> GetCategoryByIdAsync(long id, CancellationToken ct)
    {
        var result = await _service.GetCategoryByIdAsync(id, ct);
        return ToActionResult(result);
    }
    
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategoryAsync(
        [FromBody] CreateCategoryRequest request, CancellationToken ct)
    {
        var result = await _service.CreateCategoryAsync(request, ct);
        if (result.IsFailure) return ToActionResult(result);

        return ToActionResult(result, StatusCodes.Status201Created);
    }
    
    [HttpPut("categories/{id:long}")]
    public async Task<IActionResult> UpdateCategoryAsync(
        long id, [FromBody] UpdateCategoryRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateCategoryAsync(id, request, ct);
        return ToActionResult(result);
    }
    
    [HttpPut("categories/{id:long}/activate")]
    public async Task<IActionResult> ActivateCategoryAsync(long id, CancellationToken ct)
    {
        var result = await _service.ActivateCategoryAsync(id, ct);
        if (result.IsFailure) return ToActionResult(result);

        return NoContent();
    }
    
    [HttpPut("categories/{id:long}/deactivate")]
    public async Task<IActionResult> DeactivateCategoryAsync(long id, CancellationToken ct)
    {
        var result = await _service.DeactivateCategoryAsync(id, ct);
        if (result.IsFailure) return ToActionResult(result);

        return NoContent();
    }
    
    [HttpDelete("categories/{id:long}")]
    public async Task<IActionResult> DeleteCategoryAsync(long id, CancellationToken ct)
    {
        var result = await _service.DeleteCategoryAsync(id, ct);
        if (result.IsFailure) return ToActionResult(result);

        return NoContent();
    }
    
    [HttpPut("categories/reorder")]
    public async Task<IActionResult> ReorderCategoriesAsync(
        [FromBody] ReorderRequest request, CancellationToken ct)
    {
        var result = await _service.ReorderCategoriesAsync(request, ct);
        if (result.IsFailure) return ToActionResult(result);

        return NoContent();
    }
    
    #endregion
    
    #region Items
    
    [HttpGet("categories/{categoryId:long}/items")]
    public async Task<IActionResult> GetItemsByCategoryAsync(long categoryId, CancellationToken ct)
    {
        var result = await _service.GetItemsByCategoryAsync(categoryId, ct);
        return ToActionResult(result);
    }
    
    [HttpGet("categories/{categoryId:long}/items/{id:long}")]
    public async Task<IActionResult> GetItemByIdAsync(long categoryId, long id, CancellationToken ct)
    {
        var result = await _service.GetItemByIdAsync(categoryId, id, ct);
        return ToActionResult(result);
    }
    
    [HttpPost("categories/{categoryId:long}/items")]
    public async Task<IActionResult> CreateItemAsync(
        long categoryId, [FromBody] CreateItemRequest request, CancellationToken ct)
    {
        var result = await _service.CreateItemAsync(categoryId, request, ct);
        if (result.IsFailure) return ToActionResult(result);

        return ToActionResult(result, StatusCodes.Status201Created);
    }
    
    [HttpPut("categories/{categoryId:long}/items/{id:long}")]
    public async Task<IActionResult> UpdateItemAsync(
        long categoryId, long id, [FromBody] UpdateItemRequest request, CancellationToken ct)
    {
        var result = await _service.UpdateItemAsync(categoryId, id, request, ct);
        return ToActionResult(result);
    }
    
    [HttpPut("categories/{categoryId:long}/items/{id:long}/activate")]
    public async Task<IActionResult> ActivateItemAsync(long categoryId, long id, CancellationToken ct)
    {
        var result = await _service.ActivateItemAsync(categoryId, id, ct);
        if (result.IsFailure) return ToActionResult(result);

        return NoContent();
    }
    
    [HttpPut("categories/{categoryId:long}/items/{id:long}/deactivate")]
    public async Task<IActionResult> DeactivateItemAsync(long categoryId, long id, CancellationToken ct)
    {
        var result = await _service.DeactivateItemAsync(categoryId, id, ct);
        if (result.IsFailure) return ToActionResult(result);

        return NoContent();
    }
    
    [HttpDelete("categories/{categoryId:long}/items/{id:long}")]
    public async Task<IActionResult> DeleteItemAsync(long categoryId, long id, CancellationToken ct)
    {
        var result = await _service.DeleteItemAsync(categoryId, id, ct);
        if (result.IsFailure) return ToActionResult(result);

        return NoContent();
    }
    
    [HttpPut("categories/{categoryId:long}/items/reorder")]
    public async Task<IActionResult> ReorderItemsAsync(
        long categoryId, [FromBody] ReorderRequest request, CancellationToken ct)
    {
        var result = await _service.ReorderItemsAsync(categoryId, request, ct);
        if (result.IsFailure) return ToActionResult(result);

        return NoContent();
    }
    
    #endregion
    
    #region Translations
    
    [HttpGet("categories/{categoryId:long}/translations")]
    public async Task<IActionResult> GetCategoryTranslationsAsync(long categoryId, CancellationToken ct)
    {
        var result = await _service.GetCategoryTranslationsAsync(categoryId, ct);
        return ToActionResult(result);
    }
    
    [HttpPut("categories/{categoryId:long}/translations/{culture}")]
    public async Task<IActionResult> UpsertCategoryTranslationAsync(
        long categoryId, string culture,
        [FromBody] UpsertTranslationRequest request, CancellationToken ct)
    {
        var result = await _service.UpsertCategoryTranslationAsync(categoryId, culture, request, ct);
        return ToActionResult(result);
    }
    
    [HttpGet("categories/{categoryId:long}/items/{itemId:long}/translations")]
    public async Task<IActionResult> GetItemTranslationsAsync(
        long categoryId, long itemId, CancellationToken ct)
    {
        var result = await _service.GetItemTranslationsAsync(categoryId, itemId, ct);
        return ToActionResult(result);
    }
    
    [HttpPut("categories/{categoryId:long}/items/{itemId:long}/translations/{culture}")]
    public async Task<IActionResult> UpsertItemTranslationAsync(
        long categoryId, long itemId, string culture,
        [FromBody] UpsertTranslationRequest request, CancellationToken ct)
    {
        var result = await _service.UpsertItemTranslationAsync(categoryId, itemId, culture, request, ct);
        return ToActionResult(result);
    }
    
    #endregion
}