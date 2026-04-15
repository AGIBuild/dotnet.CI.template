using System.Text.Json;
using ChengYuan.ExceptionHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChengYuan.WebHost;

internal sealed partial class GlobalExceptionMiddleware(
    RequestDelegate next,
    IHostEnvironment environment,
    ILogger<GlobalExceptionMiddleware> logger)
{
    private static readonly JsonSerializerOptions ProblemJsonOptions = new(JsonSerializerDefaults.Web);

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

            var mapper = context.RequestServices.GetService<IExceptionToProblemDetailsMapper>();
            var includeSensitiveDetails = environment.IsDevelopment();

            var problemDetails = mapper is not null
                ? mapper.Map(exception, includeSensitiveDetails)
                : new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An error occurred while processing your request.",
                    Detail = includeSensitiveDetails ? exception.Message : null,
                };

            context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json; charset=utf-8";

            await JsonSerializer.SerializeAsync(context.Response.Body, problemDetails, ProblemJsonOptions, context.RequestAborted);
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "An unhandled exception has occurred outside the MVC pipeline.")]
    private static partial void LogUnhandledException(ILogger logger, Exception exception);
}
