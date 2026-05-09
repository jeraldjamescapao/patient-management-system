namespace MedCore.Infrastructure.Localization;

internal sealed class Translation
{
    public long Id { get; private set; }
    public string Culture { get; private set; } = null!;
    public string Key { get; private set; } = null!;
    public string Value { get; private set; } = null!;

    private Translation() { }

    public Translation(string culture, string key, string value)
    {
        Culture = culture;
        Key = key;
        Value = value;
    }
}