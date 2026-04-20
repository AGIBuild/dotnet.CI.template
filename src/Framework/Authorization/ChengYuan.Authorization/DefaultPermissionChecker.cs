using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ChengYuan.ExecutionContext;
using ChengYuan.Features;
using ChengYuan.MultiTenancy;

namespace ChengYuan.Authorization;

internal sealed class DefaultPermissionChecker(
    IPermissionDefinitionManager definitionManager,
    IEnumerable<IPermissionGrantProvider> grantProviders,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser,
    ICurrentCorrelation currentCorrelation,
    IFeatureChecker featureChecker) : IPermissionChecker
{
    private readonly IPermissionGrantProvider[] _orderedProviders = grantProviders
        .OrderByDescending(provider => provider.Order)
        .ThenBy(provider => provider.Name, StringComparer.Ordinal)
        .ToArray();

    public async ValueTask<bool> IsGrantedAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var definition = definitionManager.GetPermission(name);

        if (!definition.IsEnabled)
        {
            return false;
        }

        if (!IsMultiTenancySideMatch(definition))
        {
            return false;
        }

        var requiredFeatures = definition.GetRequiredFeatures();
        foreach (var feature in requiredFeatures)
        {
            if (!await featureChecker.IsEnabledAsync(feature, cancellationToken))
            {
                return false;
            }
        }

        var context = new PermissionContext(currentTenant.Id, currentUser.Id, currentCorrelation.CorrelationId, currentUser.IsAuthenticated);

        var hasGranted = false;
        foreach (var provider in _orderedProviders)
        {
            var result = await provider.CheckAsync(definition, context, cancellationToken);
            if (result == PermissionGrantResult.Prohibited)
            {
                return false;
            }

            if (result == PermissionGrantResult.Granted)
            {
                hasGranted = true;
            }
        }

        return hasGranted || definition.DefaultGranted;
    }

    private bool IsMultiTenancySideMatch(PermissionDefinition definition)
    {
        var currentSide = currentTenant.Id.HasValue
            ? MultiTenancySides.Tenant
            : MultiTenancySides.Host;

        return definition.MultiTenancySide.HasFlag(currentSide);
    }
}
