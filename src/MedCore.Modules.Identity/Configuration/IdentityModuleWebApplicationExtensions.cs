namespace MedCore.Modules.Identity.Configuration;

using Microsoft.AspNetCore.Builder;
using MedCore.Modules.Identity.Infrastructure.Persistence;

public static class IdentityModuleWebApplicationExtensions
{
    public static async Task SeedIdentityAsync(this WebApplication app)
    {
        await IdentityRoleSeeder.SeedAsync(app.Services);
    }
}