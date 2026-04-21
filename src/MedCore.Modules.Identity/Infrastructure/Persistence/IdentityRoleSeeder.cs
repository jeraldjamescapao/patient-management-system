namespace MedCore.Modules.Identity.Infrastructure.Persistence;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MedCore.Modules.Identity.Domain.Roles;

internal static class IdentityRoleSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(nameof(IdentityRoleSeeder));

        var roles = new Dictionary<string, string>
        {
            [IdentityRoles.Admin] = "System Administrator",
            [IdentityRoles.Doctor] = "Doctor User",
            [IdentityRoles.Patient] = "Patient User"
        };
        
        logger.LogInformation("Seeding identity roles: {Roles}.", string.Join(", ", roles.Keys));
        
        foreach (var role in roles)
        {
            var exists = await roleManager.RoleExistsAsync(role.Key);
            if (exists)
            {
                logger.LogDebug("Role '{Role}' already exists. Skipping.", role.Key);
                continue;
            }
            
            var applicationRole = new ApplicationRole(role.Key, role.Value);
            var result = await roleManager.CreateAsync(applicationRole);

            if (result.Succeeded)
            {
                logger.LogInformation("Role '{Role}' seeded successfully.", role.Key);
                continue;
            }
            
            var errors = string.Join(", ", result.Errors.Select(x => x.Description));
            logger.LogError("Failed to seed role '{Role}': {Errors}.", role.Key, errors);
            throw new InvalidOperationException($"Failed to seed role '{role.Key}': {errors}");
        }
    }
}