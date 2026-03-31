using Microsoft.AspNetCore.Http;

namespace PatientManagementSystem.Common.Controllers;

using Microsoft.AspNetCore.Mvc;
using PatientManagementSystem.Common.Results;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    private static readonly IReadOnlyDictionary<ResultErrorType, int> StatusCodeMap =
        new Dictionary<ResultErrorType, int>
        {
            [ResultErrorType.Validation]          = StatusCodes.Status400BadRequest,
            [ResultErrorType.Unauthorized]        = StatusCodes.Status401Unauthorized,
            [ResultErrorType.Forbidden]           = StatusCodes.Status403Forbidden,
            [ResultErrorType.NotFound]            = StatusCodes.Status404NotFound,
            [ResultErrorType.Conflict]            = StatusCodes.Status409Conflict,
            [ResultErrorType.UnprocessableEntity] = StatusCodes.Status422UnprocessableEntity,
            [ResultErrorType.Internal]            = StatusCodes.Status500InternalServerError,
            [ResultErrorType.ServiceUnavailable]  = StatusCodes.Status503ServiceUnavailable,
        };
    
    protected IActionResult ToActionResult<T>(Result<T> result)
    {
        if (result.IsSuccess) 
            return StatusCode(StatusCodes.Status200OK, result.Value);

        var error = new { code = result.Error!.Code, message = result.Error.Message };
        
        if (!StatusCodeMap.TryGetValue(result.ErrorType, out var statusCode))
            throw new InvalidOperationException($"Unhandled error type: {result.ErrorType}.");

        return StatusCode(statusCode, error);
    }
}