namespace MedCore.Modules.Users;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MedCore.Common.Modules;
using MedCore.Modules.Users.Configuration;

public sealed class UsersModule : IModule
{
    public IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddUsersModule();
        return services;
    }

    public WebApplication MapEndpoints(WebApplication app)
    {
        return app;
    }
}