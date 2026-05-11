namespace MedCore.Modules.Localization;

using MedCore.Common.Modules;
using MedCore.Modules.Localization.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public sealed class LocalizationModule : IModule
{
    public IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddLocalizationModule(configuration);
        return services;
    }

    public WebApplication MapEndpoints(WebApplication app)
    {
        return app;
    }
}