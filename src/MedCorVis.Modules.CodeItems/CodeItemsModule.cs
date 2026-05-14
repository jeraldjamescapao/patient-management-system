namespace MedCorVis.Modules.CodeItems;

using MedCorVis.Common.Modules;
using MedCorVis.Modules.CodeItems.Configuration;
using MedCorVis.Modules.CodeItems.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public sealed class CodeItemsModule : IModule
{
    public IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddCodeItemsModule(configuration);
        return services;
    }

    public WebApplication MapEndpoints(WebApplication app)
    {
        return app;
    }
    
    public async Task RunStartupTasksAsync(WebApplication app)
    {
        await CodeItemSeeder.SeedAsync(app.Services);
    }
}