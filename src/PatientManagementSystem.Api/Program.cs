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

await IdentityRoleSeeder.SeedAsync(app.Services);

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapGet("/", () => "Hello, this is James! Welcome to my humble Patient Management System API! :)");
app.MapControllers();
app.MapModuleEndpoints();

app.Run();
