namespace MedCore.Modules.Users.Presentation.Controllers;

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MedCore.Common.Controllers;
using MedCore.Common.Results;
using MedCore.Common.Services;
using MedCore.Modules.Users.Application.Abstractions;
using MedCore.Modules.Users.Application.Contracts;

[Authorize]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/users")]
public sealed class UsersController : BaseApiController
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;
    
    public UsersController(IUserService userService, ICurrentUserService currentUserService)
    {
        _userService = userService;
        _currentUserService = currentUserService;
    }
    
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUserAsync(CancellationToken ct)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var userId))
            return ToActionResult(Result<UserResponse>.Unauthorized(UserErrors.InvalidToken));

        var result = await _userService.GetCurrentUserAsync(userId, ct);
        return ToActionResult(result);
    }
    
    [HttpPut("me/culture")]
    public async Task<IActionResult> UpdateCultureAsync(
        [FromBody] UpdateCultureRequest request, CancellationToken ct)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var userId))
            return ToActionResult(Result<bool>.Unauthorized(UserErrors.InvalidToken));

        var result = await _userService.UpdateCultureAsync(userId, request.Culture, ct);
        if (result.IsFailure) return ToActionResult(result);

        return NoContent();
    }
    
    [HttpPut("me/profile")]
    public async Task<IActionResult> UpdateProfileAsync(
        [FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var userId))
            return ToActionResult(Result<UserResponse>.Unauthorized(UserErrors.InvalidToken));

        var result = await _userService.UpdateProfileAsync(userId, request, ct);
        return ToActionResult(result);
    }

    [HttpPut("me/phone")]
    public async Task<IActionResult> UpdatePhoneAsync(
        [FromBody] UpdatePhoneRequest request, CancellationToken ct)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var userId))
            return ToActionResult(Result<bool>.Unauthorized(UserErrors.InvalidToken));

        var result = await _userService.UpdatePhoneAsync(userId, request.PhoneNumber, ct);
        if (result.IsFailure) return ToActionResult(result);

        return NoContent();
    }
}