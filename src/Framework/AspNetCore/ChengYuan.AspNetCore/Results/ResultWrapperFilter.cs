using ChengYuan.Core.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ChengYuan.AspNetCore;

internal sealed class ResultWrapperFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is not ObjectResult { Value: not null } objectResult)
        {
            return;
        }

        switch (objectResult.Value)
        {
            case Result result:
                context.Result = WrapResult(result, context);
                break;

            case { } value when IsGenericResult(value.GetType()):
                context.Result = WrapGenericResult(value, context);
                break;
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    private static ObjectResult WrapResult(Result result, ResultExecutingContext context)
    {
        if (result.IsSuccess)
        {
            return new ObjectResult(ApiResponse.Ok()) { StatusCode = StatusCodes.Status200OK };
        }

        var statusCode = MapErrorTypeToStatusCode(result.Error.Type);
        var error = new ApiErrorInfo
        {
            Code = result.Error.Code,
            Message = result.Error.Message,
        };

        return new ObjectResult(ApiResponse.Fail(error)) { StatusCode = statusCode };
    }

    private static ObjectResult WrapGenericResult(object resultObj, ResultExecutingContext context)
    {
        var type = resultObj.GetType();
        var isSuccess = (bool)type.GetProperty(nameof(Result.IsSuccess))!.GetValue(resultObj)!;

        if (isSuccess)
        {
            var value = type.GetProperty("Value")!.GetValue(resultObj);
            return new ObjectResult(ApiResponse.Ok(value)) { StatusCode = StatusCodes.Status200OK };
        }

        var error = (ResultError)type.GetProperty(nameof(Result.Error))!.GetValue(resultObj)!;
        var statusCode = MapErrorTypeToStatusCode(error.Type);
        var errorInfo = new ApiErrorInfo
        {
            Code = error.Code,
            Message = error.Message,
        };

        return new ObjectResult(ApiResponse.Fail(errorInfo)) { StatusCode = statusCode };
    }

    private static bool IsGenericResult(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>);

    private static int MapErrorTypeToStatusCode(ResultErrorType errorType) => errorType switch
    {
        ResultErrorType.Validation => StatusCodes.Status422UnprocessableEntity,
        ResultErrorType.NotFound => StatusCodes.Status404NotFound,
        ResultErrorType.Conflict => StatusCodes.Status409Conflict,
        ResultErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ResultErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ResultErrorType.Unexpected => StatusCodes.Status500InternalServerError,
        _ => StatusCodes.Status400BadRequest,
    };
}
