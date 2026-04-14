using ChengYuan.AuditLogging;
using ChengYuan.EntityFrameworkCore;
using ChengYuan.FeatureManagement;
using ChengYuan.Identity;
using ChengYuan.PermissionManagement;
using ChengYuan.SettingManagement;
using ChengYuan.TenantManagement;
using Shouldly;
using Xunit;

namespace ChengYuan.FrameworkKernel.Tests.Framework.Data;

public sealed class MigrationHistoryTableNameTests
{
    [Theory]
    [InlineData(typeof(IdentityDbContext), "__EFMigrationsHistory_Identity")]
    [InlineData(typeof(TenantManagementDbContext), "__EFMigrationsHistory_TenantManagement")]
    [InlineData(typeof(AuditLoggingDbContext), "__EFMigrationsHistory_AuditLogging")]
    [InlineData(typeof(PermissionManagementDbContext), "__EFMigrationsHistory_PermissionManagement")]
    [InlineData(typeof(SettingManagementDbContext), "__EFMigrationsHistory_SettingManagement")]
    [InlineData(typeof(FeatureManagementDbContext), "__EFMigrationsHistory_FeatureManagement")]
    public void Resolve_produces_module_specific_table_name(Type dbContextType, string expectedTableName)
    {
        MigrationHistoryTableNameResolver.Resolve(dbContextType).ShouldBe(expectedTableName);
    }

    [Fact]
    public void Resolve_generic_matches_non_generic()
    {
#pragma warning disable CA2263 // Deliberately testing non-generic overload
        MigrationHistoryTableNameResolver.Resolve<IdentityDbContext>()
            .ShouldBe(MigrationHistoryTableNameResolver.Resolve(typeof(IdentityDbContext)));
#pragma warning restore CA2263
    }
}
