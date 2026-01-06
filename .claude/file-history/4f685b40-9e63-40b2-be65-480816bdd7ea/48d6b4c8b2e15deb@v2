namespace TradeCaptureSystem.Domain.Common;

/// <summary>
/// Represents the result of an operation with success/failure state and error information
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }

    protected Result(bool isSuccess, string error)
    {
        if (isSuccess && !string.IsNullOrWhiteSpace(error))
            throw new InvalidOperationException("Success result cannot have an error");

        if (!isSuccess && string.IsNullOrWhiteSpace(error))
            throw new InvalidOperationException("Failure result must have an error");

        IsSuccess = isSuccess;
        Error = error ?? string.Empty;
    }

    public static Result Success() => new Result(true, string.Empty);

    public static Result Failure(string error) => new Result(false, error);

    public static Result<T> Success<T>(T value) => new Result<T>(value, true, string.Empty);

    public static Result<T> Failure<T>(string error) => new Result<T>(default!, false, error);
}

/// <summary>
/// Represents the result of an operation with a return value
/// </summary>
public class Result<T> : Result
{
    public T Value { get; }

    protected internal Result(T value, bool isSuccess, string error)
        : base(isSuccess, error)
    {
        Value = value;
    }
}
