using MedCore.Api.Middleware;
using MedCore.Common.Configuration;
using MedCore.Common.Modules;
using MedCore.Infrastructure.Configuration;
using MedCore.Modules.Identity;
using MedCore.Modules.Identity.Configuration;
using MedCore.Modules.Localization;
using MedCore.Modules.Localization.Configuration;
using MedCore.Modules.Users;
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
        typeof(IdentityModule).Assembly,
        typeof(UsersModule).Assembly,
        typeof(LocalizationModule).Assembly);

    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddOptions<FrontendSettings>()
        .BindConfiguration(FrontendSettings.SectionName)
        .ValidateDataAnnotations()
        .ValidateOnStart();

    var app = builder.Build();
    
    await app.SeedIdentityAsync();
    await app.SeedTranslationsAsync();
    await app.WarmUpLocalizerAsync();
    
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
    app.UseSerilogRequestLogging();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    
    app.UseAuthentication();
    app.UseMiddleware<CultureMiddleware>(); 
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