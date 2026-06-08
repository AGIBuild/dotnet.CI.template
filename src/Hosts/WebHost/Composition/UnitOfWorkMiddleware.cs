using ChengYuan.Core.Data;
using Microsoft.AspNetCore.Http;

namespace ChengYuan.WebHost;

public sealed class UnitOfWorkMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IUnitOfWorkManager unitOfWorkManager)
    {
        var options = new UnitOfWorkOptions
        {
            TransactionBehavior = IsTransactionalRequest(context.Request.Method)
                ? UnitOfWorkTransactionBehavior.Enabled
                : UnitOfWorkTransactionBehavior.Disabled,
        };

        await using var unitOfWork = unitOfWorkManager.Begin(options);

        try
        {
            await next(context);

            if (context.Response.StatusCode < StatusCodes.Status400BadRequest)
            {
                await unitOfWork.CompleteAsync(context.RequestAborted);
            }
            else
            {
                await unitOfWork.RollbackAsync(context.RequestAborted);
            }
        }
        catch
        {
            await unitOfWork.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private static bool IsTransactionalRequest(string method)
        => HttpMethods.IsPost(method)
            || HttpMethods.IsPut(method)
            || HttpMethods.IsPatch(method)
            || HttpMethods.IsDelete(method);
}
