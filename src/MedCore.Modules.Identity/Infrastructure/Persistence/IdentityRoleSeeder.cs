namespace MedCore.Modules.Identity.Infrastructure.Persistence;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MedCore.Modules.Identity.Domain.Roles;

public static class IdentityRoleSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        var roles = new Dictionary<string, string>
        {
            [IdentityRoles.Admin] = "System Administrator",
            [IdentityRoles.Doctor] = "Doctor User",
            [IdentityRoles.Patient] = "Patient User"
        };
        
        foreach (var role in roles)
        {
            var exists = await roleManager.RoleExistsAsync(role.Key);
            if (exists) continue;
            
            var applicationRole = new ApplicationRole(role.Key, role.Value);
            var result = await roleManager.CreateAsync(applicationRole);

            if (result.Succeeded) continue;
            
            var errors = string.Join(", ", result.Errors.Select(x => x.Description));
            throw new InvalidOperationException($"Failed to seed role '{role.Key}': {errors}");
        }
    }
}