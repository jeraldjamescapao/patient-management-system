namespace MedCore.Modules.Identity.Configuration;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MedCore.Common.Configuration;
using MedCore.Common.Services;
using MedCore.Modules.Identity.Application.Abstractions.Authentication;
using MedCore.Modules.Identity.Application.Abstractions.Email;
using MedCore.Modules.Identity.Application.Abstractions.Persistence;
using MedCore.Modules.Identity.Application.Services;
using MedCore.Modules.Identity.Domain.Roles;
using MedCore.Modules.Identity.Domain.Tokens;
using MedCore.Modules.Identity.Domain.Users;
using MedCore.Modules.Identity.Infrastructure.Authentication;
using MedCore.Modules.Identity.Infrastructure.BackgroundServices;
using MedCore.Modules.Identity.Infrastructure.Email;
using MedCore.Modules.Identity.Infrastructure.Persistence;
using MedCore.Modules.Identity.Infrastructure.Persistence.Repositories;
using MedCore.Modules.Identity.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
    
public static class IdentityModuleServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSqlConnection") 
            ?? throw new InvalidOperationException("Database connection string is not configured.");
        
        services.AddIdentityPersistence(connectionString);
        services.AddIdentityServices(configuration);
        services.AddIdentityJwt(configuration);
        
        return services;
    }

    private static IServiceCollection AddIdentityPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<IdentityDbContext>(options =>
        {
            options
                .UseNpgsql(connectionString, 
                    o => o.MigrationsAssembly("MedCore.Modules.Identity"))
                .UseSnakeCaseNamingConvention();
        });
        
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();
        
        return services;
    }

    private static IServiceCollection AddIdentityServices(
        this IServiceCollection services,
        IConfiguration configuration)
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
        services.AddScoped<IIdentityEmailService, IdentityEmailService>();

        services.AddOptions<IdentityTokenSettings>()
            .BindConfiguration(IdentityTokenSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            var tokenSettings = configuration.GetSection(IdentityTokenSettings.SectionName)
                                    .Get<IdentityTokenSettings>()
                                ?? throw new InvalidOperationException("IdentityTokens settings are not configured.");

            options.TokenLifespan = TimeSpan.FromHours(tokenSettings.EmailConfirmationExpirationInHours);
        });
        
        services.AddOptions<RefreshTokenCleanupSettings>()
            .BindConfiguration(RefreshTokenCleanupSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHostedService<RefreshTokenCleanupService>();
        
        return services;       
    }

    private static IServiceCollection AddIdentityJwt(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<JwtSettings>()
            .BindConfiguration(JwtSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings are not configured.");
        
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });
        
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        
        return services;      
    }
}