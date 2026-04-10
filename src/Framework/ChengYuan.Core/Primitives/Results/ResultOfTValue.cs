using System;

namespace ChengYuan.Core.Results;

public sealed class Result<TValue>
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, ResultError error)
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

        _value = value;
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public ResultError Error { get; }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Failed results do not expose a value.");

    public Result<TOutput> Map<TOutput>(Func<TValue, TOutput> map)
    {
        ArgumentNullException.ThrowIfNull(map);

        return IsFailure
            ? Result.Failure<TOutput>(Error)
            : Result.Success(map(Value));
    }
}
