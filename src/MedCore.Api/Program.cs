using MedCore.Api.Middleware;
using MedCore.Common.Configuration;
using MedCore.Common.Modules;
using MedCore.Infrastructure;
using MedCore.Modules.Identity;
using MedCore.Modules.Identity.Infrastructure.Persistence;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting MedCore API by Jerald James Capao...");
    
    var builder = WebApplication.CreateBuilder(args);
    
    builder.Services.AddSerilog((services, loggerConfiguration) 
        => loggerConfiguration
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services));
    
    builder.Services.AddOpenApi();
    builder.Services.AddProblemDetails();
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
        app.MapScalarApiReference(options =>
        {
            options.WithTitle("MedCore API by Jerald James Capao");
            options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });
    }

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseSerilogRequestLogging();
    
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGet("/", () => "Bonjour! Welcome to MedCore API, by Jerald James Capao. :)");
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