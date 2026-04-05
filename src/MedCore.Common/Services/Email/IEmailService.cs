namespace MedCore.Common.Services.Email;

public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}