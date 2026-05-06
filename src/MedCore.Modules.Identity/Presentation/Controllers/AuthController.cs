namespace MedCore.Modules.Identity.Presentation.Controllers;

using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MedCore.Common.Controllers;
using MedCore.Common.Http;
using MedCore.Common.Results;
using MedCore.Common.Services;
using MedCore.Modules.Identity.Application.Abstractions.Authentication;
using MedCore.Modules.Identity.Application.Contracts.Authentication;
using MedCore.Modules.Identity.Configuration;

[ApiVersion("1")]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController : BaseApiController
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;
    private readonly JwtSettings _jwtSettings;

    public AuthController(
        IAuthService authService, 
        ICurrentUserService currentUserService,
        IOptions<JwtSettings> jwtSettings)
    {
        _authService = authService;
        _currentUserService = currentUserService;
        _jwtSettings = jwtSettings.Value;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync(
        [FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(request, ct);
        if (result.IsFailure) return ToActionResult(result);
        
        AppendRefreshTokenCookie(result.Value!.RawRefreshToken);
        return ToActionResult(result, StatusCodes.Status201Created);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(
        [FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);
        if (result.IsFailure) return ToActionResult(result);
        
        AppendRefreshTokenCookie(result.Value!.RawRefreshToken);
        return ToActionResult(result);
    }
    
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAsync(CancellationToken ct)
    {
        var token = Request.Cookies[CookieNames.RefreshToken] ?? string.Empty;
        var result = await _authService.RefreshAsync(token, ct);
        if (result.IsFailure) return ToActionResult(result);
        
        AppendRefreshTokenCookie(result.Value!.RawRefreshToken);
        return ToActionResult(result);
    }
    
    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync(CancellationToken ct)
    {
        var token = Request.Cookies[CookieNames.RefreshToken] ?? string.Empty;
        var result = await _authService.LogoutAsync(token, ct);
        if (result.IsFailure) return ToActionResult(result);
        
        DeleteRefreshTokenCookie();
        return NoContent();
    }

    [Authorize]
    [HttpPost("logout-all")]
    public async Task<IActionResult> LogoutAllAsync(CancellationToken ct)
    {
        if (!Guid.TryParse(_currentUserService.UserId, out var userId))
            return ToActionResult(Result<bool>.Unauthorized(AuthErrors.InvalidCredentials));
        
        var result = await _authService.LogoutAllAsync(userId, ct);
        if (result.IsFailure) return ToActionResult(result);
        
        DeleteRefreshTokenCookie();
        return NoContent();
    }
    
    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmailAsync(
        [FromQuery] Guid userId,
        [FromQuery] string token,
        CancellationToken ct)
    {
        var result = await _authService.ConfirmEmailAsync(userId, token, ct);
        return ToActionResult(result);
    }

    [HttpPost("resend-confirmation-email")]
    public async Task<IActionResult> ResendConfirmationEmailAsync(
        [FromBody] ResendConfirmationEmailRequest request,
        CancellationToken ct)
    {
        var result = await _authService.ResendConfirmationEmailAsync(request.Email, ct);
        return ToActionResult(result);
    }
    
    private void AppendRefreshTokenCookie(string rawRefreshToken)
    {
        Response.Cookies.Append(CookieNames.RefreshToken, rawRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays)
        });
    }

    private void DeleteRefreshTokenCookie()
    {
        Response.Cookies.Delete(CookieNames.RefreshToken);
    }
}