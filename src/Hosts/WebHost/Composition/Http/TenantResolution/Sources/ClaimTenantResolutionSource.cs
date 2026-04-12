using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.MultiTenancy;
using Microsoft.AspNetCore.Http;

namespace ChengYuan.WebHost;

/// <summary>
/// Resolves tenant from authenticated user claims. Highest priority Web source.
/// </summary>
internal sealed class ClaimTenantResolutionSource(MultiTenancyOptions options) : ITenantResolutionSource
{
    public int Order => 100;

    public bool IsAvailable(IServiceProvider serviceProvider)
    {
        var accessor = serviceProvider.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
        return accessor?.HttpContext?.User?.Identity?.IsAuthenticated == true;
    }

    public ValueTask PopulateAsync(
        TenantResolveContext context,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        if (context.HasResolved)
        {
            return ValueTask.CompletedTask;
        }

        var accessor = serviceProvider.GetService(typeof(IHttpContextAccessor)) as IHttpContextAccessor;
        var user = accessor?.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return ValueTask.CompletedTask;
        }

        foreach (var claimType in options.ClaimTypes)
        {
            var claim = user.Claims.FirstOrDefault(c =>
                string.Equals(c.Type, claimType, StringComparison.OrdinalIgnoreCase));

            if (claim is null || string.IsNullOrWhiteSpace(claim.Value))
            {
                continue;
            }

            if (Guid.TryParse(claim.Value, out var tenantId))
            {
                context.TenantId = tenantId;
                context.HasResolved = true;
                context.SourceName = nameof(ClaimTenantResolutionSource);
                return ValueTask.CompletedTask;
            }

            context.TenantName = claim.Value;
            context.SourceName = nameof(ClaimTenantResolutionSource);
            return ValueTask.CompletedTask;
        }

        return ValueTask.CompletedTask;
    }
}