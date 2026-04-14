using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ChengYuan.MultiTenancy;

/// <summary>
/// Holds the configured error handler delegate so the middleware can retrieve it from DI.
/// Registered only when <see cref="MultiTenancyBuilder.ConfigureErrorHandler"/> is called.
/// </summary>
public sealed class TenantResolutionErrorHandlerHolder(
    Func<HttpContext, TenantResolveResult, Task<bool>> handler)
{
    public Func<HttpContext, TenantResolveResult, Task<bool>> Handler { get; } = handler;
}
