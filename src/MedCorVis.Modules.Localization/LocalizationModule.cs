namespace MedCorVis.Modules.Localization;

using MedCorVis.Common.Localization;
using MedCorVis.Common.Modules;
using MedCorVis.Modules.Localization.Configuration;
using MedCorVis.Modules.Localization.Infrastructure.Persistence.Seeds;
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
    
    public async Task RunStartupTasksAsync(WebApplication app)
    {
        await TranslationSeeder.SeedAsync(app.Services);

        var cache = app.Services.GetRequiredService<ILocalizerCache>();
        await cache.LoadAsync();
    }
}