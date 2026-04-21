namespace MedCore.Modules.Identity.Configuration;

using System.ComponentModel.DataAnnotations;

public sealed class RefreshTokenCleanupSettings
{
    public const string SectionName = "RefreshTokenCleanup";

    [Range(1, int.MaxValue)] public int IntervalInHours { get; init; } = 24;
}