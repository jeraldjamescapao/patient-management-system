namespace MedCore.Modules.Localization.Configuration;

using MedCore.Common.Localization;
using MedCore.Modules.Localization.Application.Abstractions;
using MedCore.Modules.Localization.Infrastructure.Persistence;
using MedCore.Modules.Localization.Infrastructure.Persistence.Repositories;
using MedCore.Modules.Localization.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

internal static class LocalizationModuleServiceCollectionExtensions
{
    public static IServiceCollection AddLocalizationModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlServerConnection") 
            ?? throw new InvalidOperationException("Database connection string is not configured.");

        services.AddDbContext<LocalizationDbContext>(options =>
        {
            options.UseSqlServer(connectionString,
                o => o.MigrationsAssembly("MedCore.Modules.Localization"));
        });

        services.AddScoped<ITranslationRepository, TranslationRepository>();

        services.AddSingleton<DbMessageLocalizer>();
        services.AddSingleton<IMessageLocalizer>(sp =>
            sp.GetRequiredService<DbMessageLocalizer>());
        services.AddSingleton<ILocalizerCache>(sp =>
            sp.GetRequiredService<DbMessageLocalizer>());

        return services;
    }
}