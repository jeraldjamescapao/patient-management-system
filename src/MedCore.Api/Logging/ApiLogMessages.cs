namespace MedCore.Api.Logging;

public static class ApiLogMessages
{
    public static readonly Action<ILogger, string, string, Exception?> UnhandledException =
        LoggerMessage.Define<string, string>(
            LogLevel.Error,
            new EventId(1000, "UnhandledException"),
            "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}");
}