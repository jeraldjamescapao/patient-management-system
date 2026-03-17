namespace PatientManagementSystem.Modules.Identity.DependencyInjection;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PatientManagementSystem.Modules.Identity.Application.Abstractions.Authentication;
using PatientManagementSystem.Modules.Identity.Application.Services;
using PatientManagementSystem.Modules.Identity.Domain.Roles;
using PatientManagementSystem.Modules.Identity.Domain.Users;
using PatientManagementSystem.Modules.Identity.Infrastructure.Persistence;
    
public static class IdentityModuleServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSqlConnection") 
            ?? throw new InvalidOperationException("Database connection string was not found.");

        services.AddDbContext<IdentityDbContext>(options =>
        {
            options.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention();
        });

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
        
        services.AddScoped<IAuthService, AuthService>();
        
        return services;
    }
}