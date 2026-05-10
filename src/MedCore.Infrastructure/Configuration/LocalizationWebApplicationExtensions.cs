namespace MedCore.Infrastructure.Configuration;

using MedCore.Common.Localization;
using MedCore.Infrastructure.Localization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

public static class LocalizationWebApplicationExtensions
{
    public static async Task SeedTranslationsAsync(this WebApplication app)
    {
        await TranslationSeeder.SeedAsync(app.Services);
    }

    public static async Task WarmUpLocalizerAsync(this WebApplication app)
    {
        var localizer = app.Services.GetRequiredService<ILocalizerCache>();
        await localizer.LoadAsync();
    }
}