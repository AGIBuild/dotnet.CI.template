using ChengYuan.Core.Results;

namespace ChengYuan.Core.Validation;

public interface IValidationRule<in TValue>
{
    IEnumerable<ResultError> Validate(TValue value);
}
