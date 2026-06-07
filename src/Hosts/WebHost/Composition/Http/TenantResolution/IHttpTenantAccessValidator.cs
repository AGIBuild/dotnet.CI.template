using ChengYuan.MultiTenancy;
using Microsoft.AspNetCore.Http;

namespace ChengYuan.WebHost;

internal interface IHttpTenantAccessValidator
{
    ValueTask<bool> CanAccessAsync(
        HttpContext httpContext,
        TenantResolveResult tenant,
        CancellationToken cancellationToken = default);
}
