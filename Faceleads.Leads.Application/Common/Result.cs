namespace Faceleads.Leads.Application.Common;

public sealed class Result
{
    public bool Success { get; }

    public string? ErrorCode { get; }

    public string? ErrorMessage { get; }
    public Error? Error { get; private set; }

    private Result(bool success, string? errorCode, string? errorMessage)
    {
        Success = success;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static Result Ok() => new(true, null, null);

    public static Result Fail(Error error) => new(false, error.Code, error.Message) { Error = error };

}

public sealed class Result<T>
{
    public bool Success { get; }

    public T? Value { get; }

    public string? ErrorCode { get; }

    public string? ErrorMessage { get; }
    public Error? Error { get; private set; }

    private Result(bool success, T? value, string? errorCode, string? errorMessage)
    {
        Success = success;
        Value = value;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Ok(T value) => new(true, value, null, null);

    public static Result<T> Fail(Error error) => new(false, default, error.Code, error.Message) { Error = error };

}
