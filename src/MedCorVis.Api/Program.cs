using MedCorVis.Api.Configuration;
using MedCorVis.Api.Middleware;
using MedCorVis.Common.Configuration;
using MedCorVis.Common.Modules;
using MedCorVis.Infrastructure.Configuration;
using MedCorVis.Modules.CodeItems;
using MedCorVis.Modules.Identity;
using MedCorVis.Modules.Localization;
using MedCorVis.Modules.Users;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting MedCorVis API by Jerald James Capao...");
    
    var builder = WebApplication.CreateBuilder(args);
    
    builder.Services.AddSerilog((services, loggerConfiguration) 
        => loggerConfiguration
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services));
    
    builder.Services.AddOpenApi();
    builder.Services.AddProblemDetails();
    builder.Services.AddValidationProblemDetails();
    builder.Services.AddApiVersioning().AddMvc();
    
    builder.Services.RegisterModules(
        builder.Configuration,
        typeof(IdentityModule).Assembly,
        typeof(UsersModule).Assembly,
        typeof(LocalizationModule).Assembly,
        typeof(CodeItemsModule).Assembly);

    builder.Services.AddInfrastructure();

    builder.Services.AddOptions<FrontendSettings>()
        .BindConfiguration(FrontendSettings.SectionName)
        .ValidateDataAnnotations()
        .ValidateOnStart();

    var app = builder.Build();
    
    await app.RunModuleStartupTasksAsync();
    
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.WithTitle("MedCorVis API by Jerald James Capao");
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