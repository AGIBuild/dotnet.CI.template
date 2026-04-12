using ChengYuan.AuditLogging;
using ChengYuan.Core.Modularity;
using ChengYuan.FeatureManagement;
using ChengYuan.Identity;
using ChengYuan.PermissionManagement;
using ChengYuan.SettingManagement;
using ChengYuan.TenantManagement;

namespace ChengYuan.WebHost;

[DependsOn(typeof(WebHostFrameworkCompositionModule))]
[DependsOn(typeof(TenantManagementPersistenceModule))]
[DependsOn(typeof(SettingManagementPersistenceModule))]
[DependsOn(typeof(PermissionManagementPersistenceModule))]
[DependsOn(typeof(FeatureManagementPersistenceModule))]
[DependsOn(typeof(AuditLoggingPersistenceModule))]
[DependsOn(typeof(IdentityPersistenceModule))]
[DependsOn(typeof(IdentityWebModule))]
internal sealed class WebHostApplicationCompositionModule : HostModule
{
}