namespace MedCore.Modules.Identity.Application.Abstractions.Email;

using MedCore.Modules.Identity.Domain.Users;

public interface IIdentityEmailService
{
    Task SendConfirmationEmailAsync(
        ApplicationUser user, string encodedToken, CancellationToken ct = default);
}