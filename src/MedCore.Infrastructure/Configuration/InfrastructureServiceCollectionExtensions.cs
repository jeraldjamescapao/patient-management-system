namespace MedCore.Infrastructure.Configuration;

using MedCore.Common.Caching;
using MedCore.Common.Services;
using MedCore.Common.Services.Email;
using MedCore.Infrastructure.Caching;
using MedCore.Infrastructure.Localization;
using MedCore.Infrastructure.Email;
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

        services.AddMemoryCache();

        services.AddScoped<ICurrentCultureService, CurrentCultureService>();
        services.AddSingleton<IUserCultureCache, MemoryUserCultureCache>();
        
        services.AddOptions<EmailSettings>()
            .BindConfiguration(EmailSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IEmailService, EmailService>();
        
        return services;
    }
}