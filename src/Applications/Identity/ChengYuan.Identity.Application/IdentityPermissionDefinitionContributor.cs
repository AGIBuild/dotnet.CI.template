using ChengYuan.Authorization;

namespace ChengYuan.Identity;

internal sealed class IdentityPermissionDefinitionContributor : IPermissionDefinitionContributor
{
    public void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(IdentityPermissions.GroupName, "Identity");

        var users = group.AddPermission(IdentityPermissions.Users, "Users");
        users.AddChild(IdentityPermissions.UsersCreate, "Create User");
        users.AddChild(IdentityPermissions.UsersUpdate, "Update User");
        users.AddChild(IdentityPermissions.UsersDelete, "Delete User");

        var roles = group.AddPermission(IdentityPermissions.Roles, "Roles");
        roles.AddChild(IdentityPermissions.RolesCreate, "Create Role");
        roles.AddChild(IdentityPermissions.RolesUpdate, "Update Role");
        roles.AddChild(IdentityPermissions.RolesDelete, "Delete Role");
    }
}
