using ChengYuan.AuditLogging;
using ChengYuan.BackgroundJobs;
using ChengYuan.Core.Modularity;
using ChengYuan.FeatureManagement;
using ChengYuan.Identity;
using ChengYuan.PermissionManagement;
using ChengYuan.SettingManagement;
using ChengYuan.TenantManagement;

namespace ChengYuan.WebHost;

[DependsOn(typeof(BackgroundJobPersistenceModule))]
internal sealed class WebHostFrameworkCompositionModule : HostModule;

[DependsOn(typeof(IdentityWebModule))]
[DependsOn(typeof(TenantManagementWebModule))]
[DependsOn(typeof(SettingManagementWebModule))]
[DependsOn(typeof(PermissionManagementWebModule))]
[DependsOn(typeof(FeatureManagementWebModule))]
[DependsOn(typeof(AuditLoggingWebModule))]
internal sealed class WebHostApplicationCompositionModule : HostModule;

[DependsOn(typeof(IdentityPersistenceModule))]
[DependsOn(typeof(TenantManagementPersistenceModule))]
[DependsOn(typeof(SettingManagementPersistenceModule))]
[DependsOn(typeof(PermissionManagementPersistenceModule))]
[DependsOn(typeof(FeatureManagementPersistenceModule))]
[DependsOn(typeof(AuditLoggingPersistenceModule))]
internal sealed class WebHostRuntimeGlueModule : HostModule;
