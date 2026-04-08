namespace ChengYuan.Core.Validation;

public interface IObjectValidator<in TValue>
{
    ValidationResult Validate(TValue value);
}
