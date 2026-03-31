using PatientManagementSystem.Common.Configuration;
using PatientManagementSystem.Common.Middleware;
using PatientManagementSystem.Common.Modules;
using PatientManagementSystem.Infrastructure;
using PatientManagementSystem.Modules.Identity;
using PatientManagementSystem.Modules.Identity.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

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

try
{
    await IdentityRoleSeeder.SeedAsync(app.Services);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex, "Application failed to start due to Identity Role seeding error.");
    throw;
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello, this is James! Welcome to my humble Patient Management System API! :)");
app.MapControllers();
app.MapModuleEndpoints();

app.Run();
