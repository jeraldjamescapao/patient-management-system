namespace MedCore.Common.Results;

public sealed class Result<T>
{
    public T? Value { get; }
    public ResultError? Error { get; }
    public ResultErrorType ErrorType { get; }
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    
    private Result(T value)
    {
        Value = value;
        IsSuccess = true;
        ErrorType = ResultErrorType.None;
    }
    
    private Result(ResultError error, ResultErrorType errorType)
    {
        Error = error;
        ErrorType = errorType;
        IsSuccess = false;
    }
    
    public static Result<T> Success(T value) => new(value);
    public static Result<T> Validation(ResultError error) => new(error, ResultErrorType.Validation);
    public static Result<T> Unauthorized(ResultError error) => new(error, ResultErrorType.Unauthorized);
    public static Result<T> Forbidden(ResultError error) => new(error, ResultErrorType.Forbidden);
    public static Result<T> NotFound(ResultError error) => new(error, ResultErrorType.NotFound);
    public static Result<T> Conflict(ResultError error) => new(error, ResultErrorType.Conflict);
    public static Result<T> UnprocessableEntity(ResultError error) => new(error, ResultErrorType.UnprocessableEntity);
    public static Result<T> Internal(ResultError error) => new(error, ResultErrorType.Internal);
    public static Result<T> ServiceUnavailable(ResultError error) => new(error, ResultErrorType.ServiceUnavailable);
}