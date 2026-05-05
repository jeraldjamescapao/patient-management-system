namespace MedCore.Modules.Users.Configuration;

using Microsoft.Extensions.DependencyInjection;
using MedCore.Modules.Users.Application.Abstractions;
using MedCore.Modules.Users.Application.Services;

public static class UsersModuleServiceCollectionExtensions
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        
        return services;
    }
}