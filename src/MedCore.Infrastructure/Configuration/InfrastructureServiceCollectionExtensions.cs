namespace MedCore.Infrastructure.Configuration;

using MedCore.Common.Caching;
using MedCore.Common.Localization;
using MedCore.Common.Services;
using MedCore.Common.Services.Email;
using MedCore.Infrastructure.Caching;
using MedCore.Infrastructure.Localization;
using MedCore.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlServerConnection") 
            ?? throw new InvalidOperationException("Database connection string is not configured.");

        services.AddDbContext<LocalizationDbContext>(options =>
        {
            options.UseSqlServer(connectionString,
                o => o.MigrationsAssembly("MedCore.Infrastructure"));
        });
        
        services.AddMemoryCache();

        services.AddScoped<ITranslationRepository, TranslationRepository>();
        services.AddScoped<ICurrentCultureService, CurrentCultureService>();
        services.AddSingleton<IUserCultureCache, MemoryUserCultureCache>();

        services.AddSingleton<DbMessageLocalizer>();
        services.AddSingleton<IMessageLocalizer>(sp =>
            sp.GetRequiredService<DbMessageLocalizer>());
        services.AddSingleton<ILocalizerCache>(sp =>
            sp.GetRequiredService<DbMessageLocalizer>());
        
        services.AddOptions<EmailSettings>()
            .BindConfiguration(EmailSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IEmailService, EmailService>();
        
        return services;
    }
}