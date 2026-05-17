namespace MedCorVis.Api.Configuration;

using Microsoft.AspNetCore.Mvc;

internal static class ValidationProblemDetailsConfiguration
{
    internal static IServiceCollection AddValidationProblemDetails(
        this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .ToDictionary(
                        e => e.Key,
                        e => e.Value!.Errors.Select(x => x.ErrorMessage).ToArray());

                var problem = new ProblemDetails
                {
                    Type     = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    Title    = "Bad Request",
                    Status   = StatusCodes.Status400BadRequest,
                    Detail   = "One or more validation errors occurred.",
                    Instance = context.HttpContext.Request.Path,
                    Extensions =
                    {
                        ["traceId"] = context.HttpContext.TraceIdentifier,
                        ["errors"] = errors
                    }
                };

                return new ObjectResult(problem)
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            };
        });

        return services;
    }
}