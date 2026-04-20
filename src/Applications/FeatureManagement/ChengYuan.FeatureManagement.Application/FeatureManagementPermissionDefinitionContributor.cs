using ChengYuan.Authorization;

namespace ChengYuan.FeatureManagement;

internal sealed class FeatureManagementPermissionDefinitionContributor : IPermissionDefinitionContributor
{
    public void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(FeatureManagementPermissions.GroupName, "Feature Management");
        group.AddPermission(FeatureManagementPermissions.Features, "Features");
    }
}
