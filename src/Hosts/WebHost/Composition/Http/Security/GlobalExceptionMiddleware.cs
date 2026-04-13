using System.Net.Mime;
using System.Text.Json;
using ChengYuan.ExceptionHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChengYuan.WebHost;

internal sealed partial class GlobalExceptionMiddleware(RequestDelegate next, IHostEnvironment environment, ILogger<GlobalExceptionMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            LogUnhandledException(logger, exception);

            if (context.Response.HasStarted)
            {
                return;
            }

            var converter = context.RequestServices.GetService<IExceptionToErrorInfoConverter>();
            var includeSensitiveDetails = environment.IsDevelopment();

            var errorInfo = converter is not null
                ? converter.Convert(exception, includeSensitiveDetails)
                : new ErrorInfo(message: includeSensitiveDetails ? exception.Message : "An internal error occurred during your request.");

            var statusCode = exception switch
            {
                BusinessException => StatusCodes.Status400BadRequest,
                UnauthorizedAccessException => StatusCodes.Status403Forbidden,
                ArgumentException => StatusCodes.Status400BadRequest,
                TimeoutException => StatusCodes.Status504GatewayTimeout,
                OperationCanceledException => StatusCodes.Status499ClientClosedRequest,
                _ => StatusCodes.Status500InternalServerError,
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = MediaTypeNames.Application.Json;

            await JsonSerializer.SerializeAsync(context.Response.Body, new ErrorResponse(errorInfo), JsonOptions, context.RequestAborted);
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "An unhandled exception has occurred outside the MVC pipeline.")]
    private static partial void LogUnhandledException(ILogger logger, Exception exception);
}
