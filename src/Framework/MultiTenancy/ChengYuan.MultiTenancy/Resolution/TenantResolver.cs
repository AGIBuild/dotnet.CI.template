using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ChengYuan.MultiTenancy;

internal sealed class TenantResolver(
    IServiceProvider serviceProvider,
    IOptions<TenantResolveOptions> options,
    IEnumerable<ITenantResolutionSource> sources,
    ITenantResolutionStore store) : ITenantResolver
{
    public async Task<TenantResolveResult> ResolveAsync(CancellationToken cancellationToken = default)
    {
        var resolveOptions = options.Value;
        var context = new TenantResolveContext();

        // 1. Run explicitly registered contributors in configured order
        foreach (var contributorType in resolveOptions.Contributors)
        {
            if (serviceProvider.GetRequiredService(contributorType) is not ITenantResolveContributor contributor)
            {
                continue;
            }

            await contributor.ResolveAsync(context, cancellationToken);

            if (context.HasResolved)
            {
                return TenantResolveResult.Resolved(context.TenantId!.Value, context.TenantName);
            }
        }

        // 2. Run internal auto-strategy sources in priority order
        foreach (var source in sources.OrderBy(s => s.Order))
        {
            if (!source.IsAvailable(serviceProvider))
            {
                continue;
            }

            await source.PopulateAsync(context, serviceProvider, cancellationToken);

            if (context.HasResolved)
            {
                return TenantResolveResult.Resolved(context.TenantId!.Value, context.TenantName);
            }

            // A source provided a name candidate but not a Guid — attempt normalization
            if (!string.IsNullOrEmpty(context.TenantName) && !context.TenantId.HasValue)
            {
                return await NormalizeByNameAsync(context.TenantName, cancellationToken);
            }
        }

        // 3. No source provided any candidate — apply fallback if configured
        if (!context.HasCandidate && resolveOptions.FallbackTenantId.HasValue)
        {
            return TenantResolveResult.Resolved(
                resolveOptions.FallbackTenantId.Value,
                resolveOptions.FallbackTenantName);
        }

        return TenantResolveResult.Unresolved;
    }

    private async Task<TenantResolveResult> NormalizeByNameAsync(
        string tenantName, CancellationToken cancellationToken)
    {
        var record = await store.FindByNameAsync(tenantName, cancellationToken);

        if (record is null)
        {
            return TenantResolveResult.NotFound();
        }

        if (!record.IsActive)
        {
            return TenantResolveResult.InactiveTenant(record.Id, record.Name);
        }

        return TenantResolveResult.Resolved(record.Id, record.Name);
    }
}
