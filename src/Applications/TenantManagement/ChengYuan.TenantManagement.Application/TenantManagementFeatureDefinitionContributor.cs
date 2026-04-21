using ChengYuan.Features;

namespace ChengYuan.TenantManagement;

internal sealed class TenantManagementFeatureDefinitionContributor : IFeatureDefinitionContributor
{
    public void Define(IFeatureDefinitionContext context)
    {
        var group = context.AddGroup(TenantManagementFeatures.GroupName, "Tenant Management");

        group.AddFeature<int>(TenantManagementFeatures.MaxTenants, 0, "Maximum number of tenants (0 = unlimited)");
    }
}
