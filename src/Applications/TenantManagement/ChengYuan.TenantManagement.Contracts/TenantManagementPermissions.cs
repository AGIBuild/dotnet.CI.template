namespace ChengYuan.TenantManagement;

public static class TenantManagementPermissions
{
    public const string GroupName = "TenantManagement";

    public const string Tenants = GroupName + ".Tenants";
    public const string TenantsCreate = Tenants + ".Create";
    public const string TenantsUpdate = Tenants + ".Update";
    public const string TenantsDelete = Tenants + ".Delete";
}
