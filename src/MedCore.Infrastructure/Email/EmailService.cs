namespace MedCore.Infrastructure.Email;

using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MedCore.Common.Exceptions;
using MedCore.Common.Services.Email;
using System.Net.Sockets;

internal sealed class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    { 
        _settings = emailSettings.Value;
    }
    
    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        email.To.Add(MailboxAddress.Parse(message.To));
        email.Subject = message.Subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = message.HtmlBody,
            TextBody = message.PlainTextBody
        };
        email.Body = bodyBuilder.ToMessageBody();

        var secureSocket = GetSecureSocketOption(_settings.SecureSocket);
        
        using var smtp = new SmtpClient();

        try
        {
            await smtp.ConnectAsync(_settings.Host, _settings.Port, secureSocket, ct);
            await smtp.AuthenticateAsync(_settings.Username, _settings.Password, ct);
            await smtp.SendAsync(email, ct);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is SmtpCommandException
                                       or SmtpProtocolException
                                       or SslHandshakeException
                                       or AuthenticationException
                                       or SocketException
                                       or IOException)
        {
            throw new EmailDeliveryException("Failed to send e-mail.", ex);
        }
        finally
        {
            if (smtp.IsConnected)
            {
                await smtp.DisconnectAsync(true, ct);
            }
        }
    }

    private static SecureSocketOptions GetSecureSocketOption(string secureSocket)
    {
        return secureSocket switch
        {
            "SslOnConnect" => SecureSocketOptions.SslOnConnect,
            "StartTls"     => SecureSocketOptions.StartTls,
            "None"         => SecureSocketOptions.None,
            _              => SecureSocketOptions.Auto
        };
    }
}