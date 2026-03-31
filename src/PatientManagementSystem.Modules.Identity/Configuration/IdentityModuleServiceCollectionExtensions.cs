namespace PatientManagementSystem.Modules.Identity.Configuration;

using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PatientManagementSystem.Common.Services;
using PatientManagementSystem.Modules.Identity.Application.Abstractions.Authentication;
using PatientManagementSystem.Modules.Identity.Application.Services;
using PatientManagementSystem.Modules.Identity.Domain.Roles;
using PatientManagementSystem.Modules.Identity.Domain.Tokens;
using PatientManagementSystem.Modules.Identity.Domain.Users;
using PatientManagementSystem.Modules.Identity.Infrastructure.Persistence;
using PatientManagementSystem.Modules.Identity.Infrastructure.Persistence.Repositories;
using PatientManagementSystem.Modules.Identity.Infrastructure.Services;
    
public static class IdentityModuleServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSqlConnection") 
            ?? throw new InvalidOperationException("Database connection string was not found.");

        services.AddIdentityPersistence(connectionString);
        services.AddIdentityServices();
        services.AddIdentityApiVersioning();
        
        return services;
    }

    private static IServiceCollection AddIdentityPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<IdentityDbContext>(options =>
        {
            options.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention();
        });
        
        return services;
    }

    private static IServiceCollection AddIdentityServices(
        this IServiceCollection services)
    {
        services.AddDataProtection();
        
        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddDefaultTokenProviders();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        
        return services;       
    }
    
    private static IServiceCollection AddIdentityApiVersioning(
        this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        });

        return services;
    }

}