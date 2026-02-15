namespace API.Services;

public enum ServiceErrorType
{
    None,
    Validation,
    NotFound,
    Database,
    Unknown
}

public sealed record ServiceResult<T>(T? Value, ServiceErrorType ErrorType, string? ErrorMessage)
{
    public bool Success => ErrorType == ServiceErrorType.None;

    public static ServiceResult<T> Ok(T value) => new(value, ServiceErrorType.None, null);

    public static ServiceResult<T> Fail(ServiceErrorType errorType, string message) =>
        new(default, errorType, message);

    public static ServiceResult<T> NotFound() => new(default, ServiceErrorType.NotFound, null);
}
