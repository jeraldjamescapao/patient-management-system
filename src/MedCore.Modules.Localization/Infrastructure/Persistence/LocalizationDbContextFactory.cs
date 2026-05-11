namespace MedCore.Modules.Localization.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

internal sealed class LocalizationDbContextFactory : IDesignTimeDbContextFactory<LocalizationDbContext>
{
    public LocalizationDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(
            Directory.GetCurrentDirectory(), "../MedCore.Api");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("SqlServerConnection")
            ?? throw new InvalidOperationException("Database connection string was not found.");

        var optionsBuilder = new DbContextOptionsBuilder<LocalizationDbContext>();
        optionsBuilder.UseSqlServer(connectionString,
            o => o.MigrationsAssembly("MedCore.Modules.Localization"));

        return new LocalizationDbContext(optionsBuilder.Options);
    }
}