using System;
using ChengYuan.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChengYuan.ExceptionHandling;

public sealed class DefaultExceptionToProblemDetailsMapper : IExceptionToProblemDetailsMapper
{
    public ProblemDetails Map(Exception exception, bool includeSensitiveDetails)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var statusCode = GetStatusCode(exception);
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(exception, statusCode),
            Detail = GetDetail(exception, includeSensitiveDetails),
            Type = $"https://httpstatuses.io/{statusCode}",
        };

        AddExtensions(problemDetails, exception);

        return problemDetails;
    }

    private static int GetStatusCode(Exception exception)
    {
        if (exception is IHasHttpStatusCode hasStatusCode)
        {
            return hasStatusCode.StatusCode;
        }

        return exception switch
        {
            EntityNotFoundException => StatusCodes.Status404NotFound,
            BusinessException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status403Forbidden,
            ArgumentException => StatusCodes.Status400BadRequest,
            TimeoutException => StatusCodes.Status504GatewayTimeout,
            OperationCanceledException => StatusCodes.Status499ClientClosedRequest,
            _ => StatusCodes.Status500InternalServerError,
        };
    }

    private static string GetTitle(Exception exception, int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => exception is BusinessException ? "Business Rule Violation" : "Bad Request",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status499ClientClosedRequest => "Client Closed Request",
            StatusCodes.Status504GatewayTimeout => "Gateway Timeout",
            _ => "An error occurred while processing your request.",
        };
    }

    private static string? GetDetail(Exception exception, bool includeSensitiveDetails)
    {
        return exception switch
        {
            BusinessException biz => biz.Message,
            EntityNotFoundException enf => enf.Message,
            _ => includeSensitiveDetails ? exception.Message : null,
        };
    }

    private static void AddExtensions(ProblemDetails problemDetails, Exception exception)
    {
        if (exception is ChengYuanException { ErrorCode: not null } chengyuanException)
        {
            problemDetails.Extensions["errorCode"] = chengyuanException.ErrorCode.Value.Value;
        }

        if (exception is BusinessException { Details: not null } businessException)
        {
            problemDetails.Extensions["details"] = businessException.Details;
        }

        if (exception is EntityNotFoundException entityNotFound)
        {
            problemDetails.Extensions["entityType"] = entityNotFound.EntityType.FullName;

            if (entityNotFound.EntityId is not null)
            {
                problemDetails.Extensions["entityId"] = entityNotFound.EntityId.ToString();
            }
        }
    }
}
