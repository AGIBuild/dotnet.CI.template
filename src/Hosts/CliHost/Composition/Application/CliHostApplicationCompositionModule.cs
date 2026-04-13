using ChengYuan.AuditLogging;
using ChengYuan.Core.Modularity;
using ChengYuan.FeatureManagement;
using ChengYuan.Identity;
using ChengYuan.PermissionManagement;
using ChengYuan.SettingManagement;
using ChengYuan.TenantManagement;

namespace ChengYuan.CliHost;

[DependsOn(typeof(CliHostFrameworkCompositionModule))]
[DependsOn(typeof(TenantManagementPersistenceModule))]
[DependsOn(typeof(SettingManagementPersistenceModule))]
[DependsOn(typeof(PermissionManagementPersistenceModule))]
[DependsOn(typeof(FeatureManagementPersistenceModule))]
[DependsOn(typeof(AuditLoggingPersistenceModule))]
[DependsOn(typeof(IdentityPersistenceModule))]
internal sealed class CliHostApplicationCompositionModule : HostModule
{
}
