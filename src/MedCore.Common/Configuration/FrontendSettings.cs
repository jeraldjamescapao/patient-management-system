namespace MedCore.Common.Configuration;

using System.ComponentModel.DataAnnotations;

public sealed class FrontendSettings
{
    public const string SectionName = "Frontend";

    /// <summary>Use <see cref="NormalizedBaseUrl"/> when building URLs.</summary>
    [Required] public string BaseUrl { get; init; } = null!;
    
    public string NormalizedBaseUrl => BaseUrl.TrimEnd('/');
}