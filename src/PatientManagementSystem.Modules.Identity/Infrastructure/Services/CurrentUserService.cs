using System.Security.Claims;

namespace PatientManagementSystem.Modules.Identity.Infrastructure.Services;

using Microsoft.AspNetCore.Http;
using PatientManagementSystem.Common.Services;

internal sealed class CurrentUserService : ICurrentUserService
{
    private const string SystemActor = "System";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;    
    }
    
    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    
    public string UserId =>
        IsAuthenticated 
            ? (_httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) 
               ?? SystemActor) 
            : SystemActor;
}