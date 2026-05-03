namespace MedCore.Modules.Identity.Infrastructure.Email;

using Microsoft.Extensions.Options;
using MedCore.Common.Configuration;
using MedCore.Common.Services.Email;
using MedCore.Modules.Identity.Application.Abstractions.Email;
using MedCore.Modules.Identity.Configuration;
using MedCore.Modules.Identity.Domain.Users;

internal sealed class IdentityEmailService : IIdentityEmailService
{
    private readonly IEmailService _emailService;
    private readonly IdentityTokenSettings _identityTokenSettings;
    private readonly FrontendSettings _frontendSettings;

    public IdentityEmailService(
        IEmailService emailService,
        IOptions<IdentityTokenSettings> identityTokenSettings,
        IOptions<FrontendSettings> frontendSettings)
    {
        _emailService = emailService;
        _identityTokenSettings = identityTokenSettings.Value;
        _frontendSettings = frontendSettings.Value;
    }
    
    public async Task SendConfirmationEmailAsync(
        ApplicationUser user, 
        string encodedToken,
        CancellationToken ct = default)
    {
        var confirmationLink = 
            $"{_frontendSettings.NormalizedBaseUrl}{_identityTokenSettings.NormalizedEmailConfirmationPath}" +
            $"?userId={user.Id}&token={encodedToken}";
        
        var message = new EmailMessage(
            To: user.Email!,
            Subject: "Confirm your email address",
            HtmlBody: BuildHtmlBody(user.FullName, confirmationLink),
            PlainTextBody: BuildPlainTextBody(user.FullName, confirmationLink));

        await _emailService.SendAsync(message, ct);
    }

    private string BuildHtmlBody(string fullName, string confirmationLink)
    {
        return $"""
                <p>Hi {fullName},</p>
                <p>Thanks for registering. Please confirm your email address by clicking the link below.</p>
                <p><a href="{confirmationLink}">Confirm Email Address</a></p>
                <p>This link expires in {_identityTokenSettings.EmailConfirmationExpirationInHours} hours.</p>
                <p>If you did not create an account, you can safely ignore this email.</p>
                """;
    }

    private string BuildPlainTextBody(string fullName, string confirmationLink)
    {
        return $"""
                Hi {fullName},

                Thanks for registering. Please confirm your email address by visiting the link below.

                {confirmationLink}

                This link expires in {_identityTokenSettings.EmailConfirmationExpirationInHours} hours.

                If you did not create an account, you can safely ignore this email.
                """;
    }
}