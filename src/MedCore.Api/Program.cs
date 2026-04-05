using MedCore.Api.Middleware;
using MedCore.Common.Configuration;
using MedCore.Common.Modules;
using MedCore.Infrastructure;
using MedCore.Modules.Identity;
using MedCore.Modules.Identity.Infrastructure.Persistence;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Jerald James Capao's humble MedCore API...");
    
    var builder = WebApplication.CreateBuilder(args);
    
    builder.Services.AddSerilog((services, loggerConfiguration) 
        => loggerConfiguration
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services));
    
    builder.Services.AddOpenApi();
    builder.Services.AddApiVersioning().AddMvc();
    
    builder.Services.RegisterModules(
        builder.Configuration,
        typeof(IdentityModule).Assembly);

    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddOptions<FrontendSettings>()
        .BindConfiguration(FrontendSettings.SectionName)
        .ValidateDataAnnotations()
        .ValidateOnStart();

    var app = builder.Build();
    
    await IdentityRoleSeeder.SeedAsync(app.Services);
    
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseSerilogRequestLogging();
    
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGet("/", () => "Bonjour! Welcome to Jerald James Capao's humble MedCore API! :)");
    app.MapControllers();
    app.MapModuleEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}