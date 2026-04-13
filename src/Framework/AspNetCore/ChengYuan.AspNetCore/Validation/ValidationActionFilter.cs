using System.Reflection;
using ChengYuan.Core.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ChengYuan.AspNetCore;

internal sealed class ValidationActionFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var serviceProvider = context.HttpContext.RequestServices;

        foreach (var (name, value) in context.ActionArguments)
        {
            if (value is null || IsSimpleType(value.GetType()))
            {
                continue;
            }

            var validatorType = typeof(IObjectValidator<>).MakeGenericType(value.GetType());
            if (serviceProvider.GetService(validatorType) is not { } validator)
            {
                continue;
            }

            var validateMethod = validatorType.GetMethod(nameof(IObjectValidator<object>.Validate))!;
            var result = (ValidationResult)validateMethod.Invoke(validator, [value])!;

            if (result.IsInvalid)
            {
                context.Result = ToValidationProblem(result, context);
                return;
            }
        }

        await next();
    }

    private static bool IsSimpleType(Type type) =>
        type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal) ||
        type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(Guid) ||
        type == typeof(TimeSpan) || Nullable.GetUnderlyingType(type) is not null && IsSimpleType(Nullable.GetUnderlyingType(type)!);

    private static UnprocessableEntityObjectResult ToValidationProblem(ValidationResult result, ActionExecutingContext context)
    {
        var problemDetails = new ValidationProblemDetails
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "One or more validation errors occurred.",
            Instance = context.HttpContext.Request.Path,
        };

        foreach (var error in result.Errors)
        {
            var field = string.IsNullOrEmpty(error.Code) ? "$" : error.Code;

            if (!problemDetails.Errors.TryGetValue(field, out var existing))
            {
                problemDetails.Errors[field] = [error.Message];
            }
            else
            {
                problemDetails.Errors[field] = [.. existing, error.Message];
            }
        }

        return new UnprocessableEntityObjectResult(problemDetails);
    }
}
