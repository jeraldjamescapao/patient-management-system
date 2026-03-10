using PatientManagementSystem.Modules.Identity;
using PatientManagementSystem.Modules.Identity.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services
    .AddControllers()
    .AddApplicationPart(typeof(IdentityModuleMarker).Assembly);

builder.Services.AddIdentityModule(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "Hello, this is James! Welcome to my humble Patient Management System API! :)");

app.MapControllers();

app.Run();
