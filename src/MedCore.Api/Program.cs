using MedCore.Api.Middleware;
using MedCore.Common.Configuration;
using MedCore.Common.Modules;
using MedCore.Infrastructure;
using MedCore.Modules.Identity;
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
    
    await app.SeedIdentityAsync();
    
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.WithTitle("MedCore API by Jerald James Capao");
            options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });
    }

    app.UseHttpsRedirection();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseSerilogRequestLogging();
    
    app.UseAuthentication();
    app.UseAuthorization();

    var welcomeText = await File.ReadAllTextAsync("welcome.txt");
    app.MapGet("/", () => Results.Text(welcomeText));
    
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