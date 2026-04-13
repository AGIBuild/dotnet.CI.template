using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace ChengYuan.AspNetCore;

internal sealed partial class IdempotencyFilter(
    IDistributedCache cache,
    ILogger<IdempotencyFilter> logger) : IAsyncActionFilter
{
    private const string CacheKeyPrefix = "idempotency:";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var attribute = GetIdempotentAttribute(context);
        if (attribute is null)
        {
            await next();
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(attribute.HeaderName, out var keyValues)
            || string.IsNullOrWhiteSpace(keyValues.ToString()))
        {
            context.Result = new BadRequestObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = $"Missing required header '{attribute.HeaderName}' for idempotent request.",
            });
            return;
        }

        var idempotencyKey = keyValues.ToString();
        var cacheKey = $"{CacheKeyPrefix}{context.HttpContext.Request.Path}:{idempotencyKey}";

        var cached = await cache.GetStringAsync(cacheKey, context.HttpContext.RequestAborted);
        if (cached is not null)
        {
            LogDuplicateRequest(logger, idempotencyKey);
            context.Result = new ConflictObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Duplicate request detected.",
                Detail = $"A request with idempotency key '{idempotencyKey}' has already been processed.",
            });
            return;
        }

        var result = await next();

        if (result.Exception is null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(attribute.CacheSeconds),
            };
            await cache.SetStringAsync(cacheKey, "1", options, context.HttpContext.RequestAborted);
        }
    }

    private static IdempotentAttribute? GetIdempotentAttribute(ActionExecutingContext context)
    {
        return context.ActionDescriptor.EndpointMetadata
            .OfType<IdempotentAttribute>()
            .FirstOrDefault();
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Duplicate idempotent request detected for key '{IdempotencyKey}'.")]
    private static partial void LogDuplicateRequest(ILogger logger, string idempotencyKey);
}
