using ChengYuan.CliHost;
using ChengYuan.EntityFrameworkCore;
using ChengYuan.Hosting;
using ChengYuan.Identity;
using ChengYuan.TenantManagement;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class CliHostCompositionTests
{
    [Fact]
    public void CliHostSqliteComposition_ShouldUseSqliteDbContexts()
    {
        var connectionString = $"Data Source={Path.Combine(Path.GetTempPath(), $"chengyuan-cli-{Guid.NewGuid():N}.db")}";
        var services = new ServiceCollection();
        services.AddChengYuan<CliHostModule>(cy => cy
            .UseSqlite(connectionString)
            .DisableMultiTenancy()
            .AddModule<IdentityPersistenceModule>()
            .AddModule<TenantManagementPersistenceModule>()
        );

        using var serviceProvider = services.BuildServiceProvider();
        var moduleCatalog = serviceProvider.GetRequiredService<ChengYuan.Core.Modularity.ModuleCatalog>();
        var moduleNames = moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ToArray();

        moduleNames.ShouldContain(nameof(CliHostModule));
        moduleNames.ShouldContain("ChengYuanEntityFrameworkCoreSqliteModule");

        using var scope = serviceProvider.CreateScope();
        var identityProviderName = scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.ProviderName;
        var tenantProviderName = scope.ServiceProvider.GetRequiredService<TenantManagementDbContext>().Database.ProviderName;

        identityProviderName.ShouldNotBeNull();
        tenantProviderName.ShouldNotBeNull();
        identityProviderName.ShouldContain("Sqlite");
        tenantProviderName.ShouldContain("Sqlite");
    }
}
