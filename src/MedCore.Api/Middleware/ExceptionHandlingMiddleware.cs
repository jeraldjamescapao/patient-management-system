namespace MedCore.Api.Middleware;

using MedCore.Api.Logging;

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

            await WriteProblemDetailsAsync(httpContext, exception);
        }
    }

    private static async Task WriteProblemDetailsAsync(HttpContext httpContext, Exception exception)
    {
        if (httpContext.Response.HasStarted)
        {
            return;
        }

        httpContext.Response.Clear();
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        
        var problemDetailsService = httpContext.RequestServices
            .GetRequiredService<IProblemDetailsService>();
        
        await problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails =
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                Title = "Internal Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An unexpected error occurred.",
                Instance = httpContext.Request.Path,
                Extensions =
                {
                    ["traceId"] = httpContext.TraceIdentifier
                }
            }
        });
    }
}