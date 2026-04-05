namespace MedCore.Common.Results;

public enum ResultErrorType
{
    None,                // success only - unused for failures
    Validation,          // 400 - malformed request, missing fields
    Unauthorized,        // 401 - not authenticated
    Forbidden,           // 403 - authenticated but not allowed
    NotFound,            // 404 - resource does not exist
    Conflict,            // 409 - duplicate or concurrent update collision
    UnprocessableEntity, // 422 - valid request but business rules reject it
    Internal,            // 500 - unexpected failure
    ServiceUnavailable   // 503 - external dependency is down
}