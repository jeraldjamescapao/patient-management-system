namespace MedCore.Modules.Identity.Infrastructure.Persistence;

using MedCore.Common.Authorization;
using MedCore.Modules.Identity.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class AdminUserSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger(nameof(AdminUserSeeder));

        var email = configuration["AdminSeed:Email"]!;
        var password = configuration["AdminSeed:Password"]!;
        var firstName = configuration["AdminSeed:FirstName"]!;
        var lastName = configuration["AdminSeed:LastName"]!;

        AdminUserSeederLogMessages.AdminUserSeedingStarted(logger, email, null);

        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            AdminUserSeederLogMessages.AdminUserAlreadyExists(logger, email, null);
            return;
        }

        var user = ApplicationUser.Create(
            email: email,
            firstName: firstName,
            lastName: lastName,
            // BirthDate is required by the domain entity but has no meaning for a system-seeded admin account.
            birthDate: new DateOnly(1990, 1, 1),
            createdBy: SystemActors.System);

        user.EmailConfirmed = true;

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            AdminUserSeederLogMessages.AdminUserSeedFailed(logger, email, errors, null);
            throw new InvalidOperationException($"Failed to seed admin user '{email}': {errors}");
        }

        var roleResult = await userManager.AddToRoleAsync(user, AppRoles.Admin);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            AdminUserSeederLogMessages.AdminUserSeedFailed(logger, email, errors, null);
            throw new InvalidOperationException($"Failed to assign Admin role to '{email}': {errors}");
        }

        AdminUserSeederLogMessages.AdminUserSeededSuccessfully(logger, email, null);
    }
}