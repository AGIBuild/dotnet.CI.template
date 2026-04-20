using ChengYuan.Authorization;

namespace ChengYuan.PermissionManagement;

internal sealed class PermissionManagementPermissionDefinitionContributor : IPermissionDefinitionContributor
{
    public void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(PermissionManagementPermissions.GroupName, "Permission Management");
        group.AddPermission(PermissionManagementPermissions.Permissions, "Permissions");
    }
}
