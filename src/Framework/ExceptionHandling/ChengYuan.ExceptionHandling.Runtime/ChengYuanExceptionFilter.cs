using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChengYuan.ExceptionHandling;

public sealed partial class ChengYuanExceptionFilter(
    IExceptionToErrorInfoConverter converter,
    IHostEnvironment environment,
    ILogger<ChengYuanExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.ExceptionHandled)
        {
            return;
        }

        var exception = context.Exception;
        var statusCode = GetStatusCode(exception);
        var includeSensitiveDetails = environment.IsDevelopment();
        var errorInfo = converter.Convert(exception, includeSensitiveDetails);

        LogException(exception, statusCode);

        context.Result = new ObjectResult(new ErrorResponse(errorInfo))
        {
            StatusCode = statusCode
        };

        context.ExceptionHandled = true;
    }

    private static int GetStatusCode(Exception exception)
    {
        if (exception is IHasHttpStatusCode hasStatusCode)
        {
            return hasStatusCode.StatusCode;
        }

        return exception switch
        {
            BusinessException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };
    }

    private void LogException(Exception exception, int statusCode)
    {
        if (exception is IHasLogLevel hasLogLevel)
        {
            LogAtLevel(logger, hasLogLevel.LogLevel, exception);
            return;
        }

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            LogUnhandledException(logger, exception);
        }
        else
        {
            LogHandledException(logger, exception);
        }
    }

    private static void LogAtLevel(ILogger loggerInstance, LogLevel level, Exception exception)
    {
#pragma warning disable CA1848 // LogLevel is determined at runtime from IHasLogLevel
        loggerInstance.Log(level, exception, "An exception has occurred while executing the request.");
#pragma warning restore CA1848
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "An unhandled exception has occurred while executing the request.")]
    private static partial void LogUnhandledException(ILogger logger, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "A handled exception has occurred while executing the request.")]
    private static partial void LogHandledException(ILogger logger, Exception exception);
}
