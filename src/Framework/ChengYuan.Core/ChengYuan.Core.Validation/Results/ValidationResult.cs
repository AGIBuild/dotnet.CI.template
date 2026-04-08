using ChengYuan.Core.Results;

namespace ChengYuan.Core.Validation;

public sealed class ValidationResult
{
    public static ValidationResult Success { get; } = new([]);

    public ValidationResult(IEnumerable<ResultError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        var materializedErrors = errors
            .Where(static error => !error.IsNone)
            .ToArray();

        if (materializedErrors.Any(static error => error.Type != ResultErrorType.Validation))
        {
            throw new InvalidOperationException("Validation results can only contain validation errors.");
        }

        Errors = materializedErrors;
    }

    public IReadOnlyList<ResultError> Errors { get; }

    public bool IsValid => Errors.Count == 0;

    public bool IsInvalid => !IsValid;

    public Result ToResult()
    {
        return IsValid ? Result.Success() : Result.Failure(Errors[0]);
    }
}
