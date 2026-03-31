namespace PatientManagementSystem.Common.Configuration;

using System.ComponentModel.DataAnnotations;

public sealed class FrontendSettings
{
    public const string SectionName = "Frontend";

    [Required] public string BaseUrl { get; init; } = null!;
}