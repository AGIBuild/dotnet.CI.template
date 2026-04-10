using ChengYuan.AuditLogging;
using ChengYuan.Caching;
using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.FeatureManagement;
using ChengYuan.Identity;
using ChengYuan.MultiTenancy;
using ChengYuan.PermissionManagement;
using ChengYuan.SettingManagement;
using ChengYuan.TenantManagement;

namespace ChengYuan.WebHost;

[DependsOn(typeof(ExecutionContextModule))]
[DependsOn(typeof(MultiTenancyModule))]
[DependsOn(typeof(MemoryCachingModule))]
[DependsOn(typeof(TenantManagementPersistenceModule))]
[DependsOn(typeof(SettingManagementPersistenceModule))]
[DependsOn(typeof(PermissionManagementPersistenceModule))]
[DependsOn(typeof(FeatureManagementPersistenceModule))]
[DependsOn(typeof(AuditLoggingPersistenceModule))]
[DependsOn(typeof(IdentityPersistenceModule))]
[DependsOn(typeof(IdentityWebModule))]
internal sealed class WebHostModule : ModuleBase
{
}
