using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;

namespace ChengYuan.Authorization;

internal sealed class DefaultPermissionChecker(
    IPermissionDefinitionManager definitionManager,
    IEnumerable<IPermissionGrantProvider> grantProviders,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser,
    ICurrentCorrelation currentCorrelation) : IPermissionChecker
{
    private readonly IPermissionGrantProvider[] _orderedProviders = grantProviders
        .OrderByDescending(provider => provider.Order)
        .ThenBy(provider => provider.Name, StringComparer.Ordinal)
        .ToArray();

    public async ValueTask<bool> IsGrantedAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var definition = definitionManager.GetDefinition(name);
        var context = new PermissionContext(currentTenant.Id, currentUser.Id, currentCorrelation.CorrelationId, currentUser.IsAuthenticated);

        foreach (var provider in _orderedProviders)
        {
            var grant = await provider.GetOrNullAsync(definition, context, cancellationToken);
            if (grant is not null)
            {
                return grant.IsGranted;
            }
        }

        return definition.DefaultGranted;
    }
}
