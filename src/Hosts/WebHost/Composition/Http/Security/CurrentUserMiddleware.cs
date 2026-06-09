using ChengYuan.ExecutionContext;
using ChengYuan.Identity;
using Microsoft.AspNetCore.Http;

namespace ChengYuan.WebHost;

internal sealed class CurrentUserMiddleware(RequestDelegate next)
{
    private const string JwtSubjectClaimType = "sub";
    private const string JwtNameClaimType = "name";

    public async Task Invoke(
        HttpContext context,
        ICurrentUserAccessor currentUserAccessor,
        IUserSessionValidator userSessionValidator)
    {
        var principal = context.User;

        if (principal.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        var userId = principal.FindFirst(JwtSubjectClaimType)?.Value
            ?? principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userId, out var resolvedUserId)
            || !await userSessionValidator.IsActiveSessionAsync(resolvedUserId, context.RequestAborted))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        var userName = principal.FindFirst(JwtNameClaimType)?.Value
            ?? principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

        using (currentUserAccessor.Change(new CurrentUserInfo(userId, userName, IsAuthenticated: true)))
        {
            await next(context);
        }
    }
}
