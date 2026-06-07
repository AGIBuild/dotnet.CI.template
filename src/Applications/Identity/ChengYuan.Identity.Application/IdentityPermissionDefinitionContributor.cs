using ChengYuan.Authorization;

namespace ChengYuan.Identity;

internal sealed class IdentityPermissionDefinitionContributor : IPermissionDefinitionContributor
{
    public void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(IdentityPermissions.GroupName, "Identity");

        var users = group.AddPermission(IdentityPermissions.Users, "Users",
            multiTenancySide: MultiTenancySides.Host);
        users.AddChild(IdentityPermissions.UsersCreate, "Create User",
            multiTenancySide: MultiTenancySides.Host);
        users.AddChild(IdentityPermissions.UsersUpdate, "Update User",
            multiTenancySide: MultiTenancySides.Host);
        users.AddChild(IdentityPermissions.UsersDelete, "Delete User",
            multiTenancySide: MultiTenancySides.Host);
        users.AddChild(IdentityPermissions.UsersManageTenants, "Manage User Tenants",
            multiTenancySide: MultiTenancySides.Host);

        var roles = group.AddPermission(IdentityPermissions.Roles, "Roles",
            multiTenancySide: MultiTenancySides.Host);
        roles.AddChild(IdentityPermissions.RolesCreate, "Create Role",
            multiTenancySide: MultiTenancySides.Host);
        roles.AddChild(IdentityPermissions.RolesUpdate, "Update Role",
            multiTenancySide: MultiTenancySides.Host);
        roles.AddChild(IdentityPermissions.RolesDelete, "Delete Role",
            multiTenancySide: MultiTenancySides.Host);
    }
}
