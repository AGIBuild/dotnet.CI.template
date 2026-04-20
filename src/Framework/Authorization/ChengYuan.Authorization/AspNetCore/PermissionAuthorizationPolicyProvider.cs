using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ChengYuan.Authorization;

public sealed class PermissionAuthorizationPolicyProvider(
    IPermissionDefinitionManager permissionDefinitionManager,
    IOptions<AuthorizationOptions> options) : DefaultAuthorizationPolicyProvider(options)
{
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(policyName);

        var policy = await base.GetPolicyAsync(policyName);
        if (policy is not null)
        {
            return policy;
        }

        if (!permissionDefinitionManager.IsDefined(policyName))
        {
            return null;
        }

        var permission = permissionDefinitionManager.GetOrNull(policyName);
        if (permission is not null && !permission.IsEnabled)
        {
            return new AuthorizationPolicyBuilder()
                .RequireAssertion(_ => false)
                .Build();
        }

        return new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();
    }
}
