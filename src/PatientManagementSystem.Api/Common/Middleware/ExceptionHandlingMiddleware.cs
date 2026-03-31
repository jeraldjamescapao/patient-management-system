namespace PatientManagementSystem.Common.Middleware;

using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using PatientManagementSystem.Common.Logging;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (OperationCanceledException) when (httpContext.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected...
            httpContext.Response.StatusCode = 499;
        }
        catch (Exception exception)
        {
            ApiLogMessages.UnhandledException(
                _logger,
                httpContext.TraceIdentifier,
                httpContext.Request.Path.Value ?? string.Empty,
                exception);

            await WriteProblemsDetailsAsync(httpContext);
        }
    }

    private static async Task WriteProblemsDetailsAsync(HttpContext httpContext)
    {
        if (httpContext.Response.HasStarted)
        {
            return;
        }

        httpContext.Response.Clear();
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred.",
            Instance = httpContext.Request.Path,
            Extensions =
            {
                ["traceId"] = httpContext.TraceIdentifier
            }
        };

        var json = JsonSerializer.Serialize(problemDetails);

        await httpContext.Response.WriteAsync(json);
    }
}