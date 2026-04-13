using ChengYuan.ExecutionContext;
using Microsoft.AspNetCore.Http;

namespace ChengYuan.WebHost;

internal sealed class CurrentUserMiddleware(RequestDelegate next)
{
    private const string JwtSubjectClaimType = "sub";
    private const string JwtNameClaimType = "name";

    public async Task Invoke(HttpContext context, ICurrentUserAccessor currentUserAccessor)
    {
        var principal = context.User;

        if (principal.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        var userId = principal.FindFirst(JwtSubjectClaimType)?.Value
            ?? principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var userName = principal.FindFirst(JwtNameClaimType)?.Value
            ?? principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

        using (currentUserAccessor.Change(new CurrentUserInfo(userId, userName, IsAuthenticated: true)))
        {
            await next(context);
        }
    }
}
