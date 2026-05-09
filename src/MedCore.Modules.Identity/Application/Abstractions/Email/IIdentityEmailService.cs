namespace MedCore.Modules.Identity.Application.Abstractions.Email;

using MedCore.Modules.Identity.Domain.Users;

internal interface IIdentityEmailService
{
    Task SendConfirmationEmailAsync(
        ApplicationUser user, 
        string encodedToken, 
        string culture,
        CancellationToken ct = default);
}