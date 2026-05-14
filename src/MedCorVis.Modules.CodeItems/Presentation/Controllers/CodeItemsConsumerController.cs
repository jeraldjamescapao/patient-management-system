namespace MedCorVis.Modules.CodeItems.Presentation.Controllers;

using Asp.Versioning;
using MedCorVis.Common.Controllers;
using MedCorVis.Modules.CodeItems.Application.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/code-items")]
public sealed class CodeItemsConsumerController : BaseApiController
{
    private readonly ICodeItemService _service;

    public CodeItemsConsumerController(ICodeItemService service)
    {
        _service = service;
    }

    [HttpGet("{categoryCode}")]
    public async Task<IActionResult> GetActiveItemsAsync(string categoryCode, CancellationToken ct)
    {
        var result = await _service.GetActiveItemsAsync(categoryCode, ct);
        return ToActionResult(result);
    }
}