namespace MedCore.Common.Localization;

public static class SupportedCultures
{
    public const string English = "en";
    public const string French = "fr";
    public const string German = "de";
    public const string FrenchSwitzerland = "fr-CH";
    public const string GermanSwitzerland = "de-CH";

    public static readonly IReadOnlyList<string> All =
        [English, French, German, FrenchSwitzerland, GermanSwitzerland];

    public const string Default = English;
}