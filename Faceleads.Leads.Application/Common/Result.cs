namespace Faceleads.Leads.Application.Common;

public sealed class Result
{
    public bool Success { get; }

    public string? ErrorCode { get; }

    public string? ErrorMessage { get; }

    private Result(bool success, string? errorCode, string? errorMessage)
    {
        Success = success;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static Result Ok() => new(true, null, null);

    public static Result Fail(string errorCode, string errorMessage) => new(false, errorCode, errorMessage);
}

public sealed class Result<T>
{
    public bool Success { get; }

    public T? Value { get; }

    public string? ErrorCode { get; }

    public string? ErrorMessage { get; }

    private Result(bool success, T? value, string? errorCode, string? errorMessage)
    {
        Success = success;
        Value = value;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Ok(T value) => new(true, value, null, null);

    public static Result<T> Fail(string errorCode, string errorMessage) => new(false, default, errorCode, errorMessage);
}
