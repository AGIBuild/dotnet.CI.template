namespace ChengYuan.Identity;

public static class IdentityPermissions
{
    public const string GroupName = "Identity";

    public const string Users = GroupName + ".Users";
    public const string UsersCreate = Users + ".Create";
    public const string UsersUpdate = Users + ".Update";
    public const string UsersDelete = Users + ".Delete";

    public const string Roles = GroupName + ".Roles";
    public const string RolesCreate = Roles + ".Create";
    public const string RolesUpdate = Roles + ".Update";
    public const string RolesDelete = Roles + ".Delete";
}
