namespace MedCore.Api.Logging;

internal static class ApiLogMessages
{
    public static readonly Action<ILogger, string, string, Exception?> UnhandledException =
        LoggerMessage.Define<string, string>(
            LogLevel.Error,
            new EventId(1000, "UnhandledException"),
            "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}");
    
    public static readonly Action<ILogger, string, string, string, Exception?> DomainRuleViolation =
        LoggerMessage.Define<string, string, string>(
            LogLevel.Warning,
            new EventId(1001, "DomainRuleViolation"),
            "Domain rule violated. TraceId: {TraceId}, Path: {Path}, Code: {Code}");
}