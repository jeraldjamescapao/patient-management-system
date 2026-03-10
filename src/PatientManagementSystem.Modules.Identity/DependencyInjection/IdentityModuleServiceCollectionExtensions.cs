namespace PatientManagementSystem.Modules.Identity.DependencyInjection;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using PatientManagementSystem.Modules.Identity.Application.Interfaces;
using PatientManagementSystem.Modules.Identity.Application.Services;
using PatientManagementSystem.Modules.Identity.Infrastructure.Persistence;
using PatientManagementSystem.Modules.Identity.Infrastructure.Persistence.Repositories;
using PatientManagementSystem.Modules.Identity.Infrastructure.Services;
    
public static class IdentityModuleServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IAuthService, AuthService>();
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PostgreSqlConnection")));
        
        return services;
    }
}