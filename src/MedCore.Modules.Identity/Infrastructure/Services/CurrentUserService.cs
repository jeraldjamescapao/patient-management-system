namespace MedCore.Modules.Identity.Infrastructure.Services;

using Microsoft.AspNetCore.Http;
using MedCore.Common.Authorization;
using MedCore.Common.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

internal sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;    
    }
    
    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    
    public string UserId =>
        IsAuthenticated 
            ? (_httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub) 
               ?? SystemActors.System) 
            : SystemActors.System;
}