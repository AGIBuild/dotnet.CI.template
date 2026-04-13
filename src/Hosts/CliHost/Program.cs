using ChengYuan.AuditLogging;
using ChengYuan.CliHost;
using ChengYuan.EntityFrameworkCore;
using ChengYuan.FeatureManagement;
using ChengYuan.Hosting;
using ChengYuan.Identity;
using ChengYuan.PermissionManagement;
using ChengYuan.SettingManagement;
using ChengYuan.TenantManagement;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.AddChengYuan<CliHostModule>(cy => cy
    .UseSqlite("Data Source=chengyuan-clihost.db")
    .DisableMultiTenancy()
    .AddModule<IdentityPersistenceModule>()
    .AddModule<TenantManagementPersistenceModule>()
    .AddModule<SettingManagementPersistenceModule>()
    .AddModule<PermissionManagementPersistenceModule>()
    .AddModule<FeatureManagementPersistenceModule>()
    .AddModule<AuditLoggingPersistenceModule>()
);

await builder.Build().RunAsync();
