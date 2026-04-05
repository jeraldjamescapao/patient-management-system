namespace MedCore.Common.Services.Email;

public sealed record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    string PlainTextBody);