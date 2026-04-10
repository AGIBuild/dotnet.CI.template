using System;

namespace ChengYuan.Core.Results;

public sealed class Result
{
    private Result(bool isSuccess, ResultError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (isSuccess && !error.IsNone)
        {
            throw new ArgumentException("Successful results cannot contain an error.", nameof(error));
        }

        if (!isSuccess && error.IsNone)
        {
            throw new ArgumentException("Failed results must contain an error.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public ResultError Error { get; }

    public static Result Success() => new(true, ResultError.None);

    public static Result Failure(ResultError error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, ResultError.None);

    public static Result<TValue> Failure<TValue>(ResultError error) => new(default, false, error);
}
