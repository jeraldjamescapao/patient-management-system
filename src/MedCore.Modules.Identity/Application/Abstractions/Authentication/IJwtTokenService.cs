namespace MedCore.Modules.Identity.Application.Abstractions.Authentication;

using MedCore.Modules.Identity.Domain.Users;

internal interface IJwtTokenService
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
}