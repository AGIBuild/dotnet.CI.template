using ChengYuan.Authorization;

namespace ChengYuan.TenantManagement;

internal sealed class TenantManagementPermissionDefinitionContributor : IPermissionDefinitionContributor
{
    public void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(TenantManagementPermissions.GroupName, "Tenant Management");

        var tenants = group.AddPermission(TenantManagementPermissions.Tenants, "Tenants",
            multiTenancySide: MultiTenancySides.Host);
        tenants.AddChild(TenantManagementPermissions.TenantsCreate, "Create Tenant",
            multiTenancySide: MultiTenancySides.Host);
        tenants.AddChild(TenantManagementPermissions.TenantsUpdate, "Update Tenant",
            multiTenancySide: MultiTenancySides.Host);
        tenants.AddChild(TenantManagementPermissions.TenantsDelete, "Delete Tenant",
            multiTenancySide: MultiTenancySides.Host);
    }
}
