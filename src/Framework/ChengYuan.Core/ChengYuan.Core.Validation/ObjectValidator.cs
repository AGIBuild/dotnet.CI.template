using ChengYuan.Core.Results;

namespace ChengYuan.Core.Validation;

public sealed class ObjectValidator<TValue>(IEnumerable<IValidationRule<TValue>> rules) : IObjectValidator<TValue>
{
    private readonly IValidationRule<TValue>[] _rules = rules.ToArray();

    public ValidationResult Validate(TValue value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (_rules.Length == 0)
        {
            return ValidationResult.Success;
        }

        List<ResultError>? errors = null;

        foreach (var rule in _rules)
        {
            foreach (var error in rule.Validate(value))
            {
                if (error.IsNone)
                {
                    continue;
                }

                errors ??= [];
                errors.Add(error);
            }
        }

        return errors is null ? ValidationResult.Success : new ValidationResult(errors);
    }
}
