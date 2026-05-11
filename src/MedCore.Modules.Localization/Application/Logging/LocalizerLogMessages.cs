namespace MedCore.Modules.Localization.Application.Logging;

using Microsoft.Extensions.Logging;

internal static class LocalizerLogMessages
{
    public static readonly Action<ILogger, Exception?> TranslationSeedingStarted =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(4001, "TranslationSeedingStarted"),
            "Seeding translations...");

    public static readonly Action<ILogger, int, int, Exception?> TranslationSeedingCompleted =
        LoggerMessage.Define<int, int>(
            LogLevel.Information,
            new EventId(4002, "TranslationSeedingCompleted"),
            "Translation seeding complete. Seeded: {Seeded}, Skipped: {Skipped}.");
    
    public static readonly Action<ILogger, string, string, Exception?> TranslationCacheEmpty =
        LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            new EventId(4003, "TranslationCacheEmpty"),
            "Translation cache is empty when resolving key '{Key}' for culture '{Culture}'. Returning key as fallback.");

    public static readonly Action<ILogger, string, string, Exception?> TranslationMissing =
        LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            new EventId(4004, "TranslationMissing"),
            "Missing translation for key '{Key}' in culture '{Culture}'. Returning key as fallback.");

    public static readonly Action<ILogger, int, Exception?> TranslationCacheLoaded =
        LoggerMessage.Define<int>(
            LogLevel.Information,
            new EventId(4005, "TranslationCacheLoaded"),
            "Translation cache loaded: {CultureCount} culture(s).");

    public static readonly Action<ILogger, Exception?> TranslationCacheInvalidated =
        LoggerMessage.Define(
            LogLevel.Information,
            new EventId(4006, "TranslationCacheInvalidated"),
            "Translation cache invalidated.");
}