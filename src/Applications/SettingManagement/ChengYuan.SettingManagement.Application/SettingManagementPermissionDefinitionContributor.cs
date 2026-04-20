using ChengYuan.Authorization;

namespace ChengYuan.SettingManagement;

internal sealed class SettingManagementPermissionDefinitionContributor : IPermissionDefinitionContributor
{
    public void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(SettingManagementPermissions.GroupName, "Setting Management");
        group.AddPermission(SettingManagementPermissions.Settings, "Settings");
    }
}
