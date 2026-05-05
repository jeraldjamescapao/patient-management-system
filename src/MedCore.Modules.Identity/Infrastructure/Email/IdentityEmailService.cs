namespace MedCore.Modules.Identity.Infrastructure.Email;

using Microsoft.Extensions.Options;
using MedCore.Common.Configuration;
using MedCore.Common.Localization;
using MedCore.Common.Services.Email;
using MedCore.Modules.Identity.Application.Abstractions.Email;
using MedCore.Modules.Identity.Configuration;
using MedCore.Modules.Identity.Domain.Users;

internal sealed class IdentityEmailService : IIdentityEmailService
{
    private readonly IEmailService _emailService;
    private readonly IMessageLocalizer _localizer;
    private readonly IdentityTokenSettings _identityTokenSettings;
    private readonly FrontendSettings _frontendSettings;

    public IdentityEmailService(
        IEmailService emailService,
        IMessageLocalizer localizer,
        IOptions<IdentityTokenSettings> identityTokenSettings,
        IOptions<FrontendSettings> frontendSettings)
    {
        _emailService = emailService;
        _localizer = localizer;
        _identityTokenSettings = identityTokenSettings.Value;
        _frontendSettings = frontendSettings.Value;
    }
    
    public async Task SendConfirmationEmailAsync(
        ApplicationUser user, 
        string encodedToken,
        string culture,
        CancellationToken ct = default)
    {
        var confirmationLink = 
            $"{_frontendSettings.NormalizedBaseUrl}{_identityTokenSettings.NormalizedEmailConfirmationPath}" +
            $"?userId={user.Id}&token={encodedToken}";
        
        var message = new EmailMessage(
            To: user.Email!,
            Subject: _localizer.Get("email.confirmation.subject", culture),
            HtmlBody: BuildHtmlBody(user.FullName, confirmationLink, culture),
            PlainTextBody: BuildPlainTextBody(user.FullName, confirmationLink, culture));

        await _emailService.SendAsync(message, ct);
    }

    private string BuildHtmlBody(string fullName, string confirmationLink, string culture)
    {
        var hours = _identityTokenSettings.EmailConfirmationExpirationInHours;
        
        return $"""
                <p>{string.Format(_localizer.Get("email.confirmation.greeting", culture), fullName)}</p>
                <p>{_localizer.Get("email.confirmation.instruction", culture)}</p>
                <p><a href="{confirmationLink}">{_localizer.Get("email.confirmation.link_label", culture)}</a></p>
                <p>{string.Format(_localizer.Get("email.confirmation.expiry", culture), hours)}</p>
                <p>{_localizer.Get("email.confirmation.ignore", culture)}</p>
                """;
    }

    private string BuildPlainTextBody(string fullName, string confirmationLink, string culture)
    {
        var hours = _identityTokenSettings.EmailConfirmationExpirationInHours;
        
        return $"""
                {string.Format(_localizer.Get("email.confirmation.greeting", culture), fullName)}

                {_localizer.Get("email.confirmation.instruction", culture)}

                {confirmationLink}

                {string.Format(_localizer.Get("email.confirmation.expiry", culture), hours)}

                {_localizer.Get("email.confirmation.ignore", culture)}
                """;
    }
}