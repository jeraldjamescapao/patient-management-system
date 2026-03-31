using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PatientManagementSystem.Common.Modules;

public static class ModuleExtensions
{
    public static IServiceCollection RegisterModules(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies)
    {
        var registry = new ModuleRegistry();

        var moduleTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(IModule).IsAssignableFrom(type)
                           && type is { IsAbstract: false, IsInterface: false });

        foreach (var moduleType in moduleTypes)
        {
            var module = (IModule)Activator.CreateInstance(moduleType)!;
            module.RegisterModule(services, configuration);
            services.AddControllers().AddApplicationPart(moduleType.Assembly);
            registry.Register(module);
        }
        
        services.AddSingleton(registry);
        
        return services;
    }

    public static WebApplication MapModuleEndpoints(this WebApplication app)
    {
        var registry = app.Services.GetRequiredService<ModuleRegistry>();
        
        foreach (var module in registry.Modules)
            module.MapEndpoints(app);
                
        return app;
    }
}