using ChengYuan.AuditLogging;
using ChengYuan.Core.Data;
using ChengYuan.Core.Modularity;
using ChengYuan.EntityFrameworkCore;
using ChengYuan.FeatureManagement;
using ChengYuan.Hosting;
using ChengYuan.Identity;
using ChengYuan.MultiTenancy;
using ChengYuan.PermissionManagement;
using ChengYuan.SettingManagement;
using ChengYuan.TenantManagement;
using ChengYuan.WebHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class WebHostCompositionTests
{
    [Fact]
    public void WebHostSqliteComposition_ShouldRegisterSqliteProviderModuleAndUseSqliteDbContexts()
    {
        var connectionString = $"Data Source={Path.Combine(Path.GetTempPath(), $"chengyuan-web-{Guid.NewGuid():N}.db")}";
        var services = new ServiceCollection();
        services.UseSqlite(connectionString);
        services.AddTestWebHost();

        using var serviceProvider = services.BuildServiceProvider();
        var moduleCatalog = serviceProvider.GetRequiredService<ChengYuan.Core.Modularity.ModuleCatalog>();
        var moduleNames = moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ToArray();

        moduleNames.ShouldContain(nameof(WebHostHttpCompositionModule));
        moduleNames.ShouldContain("ChengYuanEntityFrameworkCoreSqliteModule");

        using var scope = serviceProvider.CreateScope();
        var identityProviderName = scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.ProviderName;
        var tenantProviderName = scope.ServiceProvider.GetRequiredService<TenantManagementDbContext>().Database.ProviderName;

        identityProviderName.ShouldNotBeNull();
        tenantProviderName.ShouldNotBeNull();
        identityProviderName.ShouldContain("Sqlite");
        tenantProviderName.ShouldContain("Sqlite");
    }

    [Fact]
    public void WebHostComposition_ShouldRegisterIdentityWebAndPersistenceBackedModules()
    {
        var databaseName = $"composition-{Guid.NewGuid():N}";
        var services = new ServiceCollection();
        services.UseDbContextOptions(options => options.UseInMemoryDatabase(databaseName));
        services.AddTestWebHost();

        using var serviceProvider = services.BuildServiceProvider();
        var moduleCatalog = serviceProvider.GetRequiredService<ChengYuan.Core.Modularity.ModuleCatalog>();
        var moduleNames = moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ToArray();

        moduleNames.ShouldContain(nameof(WebHostHttpCompositionModule));
        moduleNames.ShouldContain(nameof(IdentityWebModule));
        moduleNames.ShouldContain(nameof(IdentityPersistenceModule));
        moduleNames.ShouldContain(nameof(TenantManagementPersistenceModule));
        moduleNames.ShouldContain(nameof(SettingManagementPersistenceModule));
        moduleNames.ShouldContain(nameof(PermissionManagementPersistenceModule));
        moduleNames.ShouldContain(nameof(FeatureManagementPersistenceModule));
        moduleNames.ShouldContain(nameof(AuditLoggingPersistenceModule));

        using var scope = serviceProvider.CreateScope();
        scope.ServiceProvider.GetRequiredService<IdentityDbContext>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<TenantManagementDbContext>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<SettingManagementDbContext>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<PermissionManagementDbContext>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<FeatureManagementDbContext>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<AuditLoggingDbContext>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<IUserManager>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<IRoleManager>().ShouldNotBeNull();
    }

    [Fact]
    public async Task WebHostComposition_ShouldCommitChangesAcrossModuleDbContexts()
    {
        var databaseName = $"composition-uow-{Guid.NewGuid():N}";
        var services = new ServiceCollection();
        services.UseDbContextOptions(options => options.UseInMemoryDatabase(databaseName));
        services.AddTestWebHost();

        await using var serviceProvider = services.BuildServiceProvider();

        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var identityDbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            var tenantDbContext = scope.ServiceProvider.GetRequiredService<TenantManagementDbContext>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await identityDbContext.Roles.AddAsync(new IdentityRole(Guid.NewGuid(), "Administrators"), TestContext.Current.CancellationToken);
            await tenantDbContext.Tenants.AddAsync(new TenantEntity(Guid.NewGuid(), "Default", isActive: true), TestContext.Current.CancellationToken);

            await unitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var verificationScope = serviceProvider.CreateAsyncScope())
        {
            var identityDbContext = verificationScope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            var tenantDbContext = verificationScope.ServiceProvider.GetRequiredService<TenantManagementDbContext>();

            var roleNames = await identityDbContext.Roles.Select(role => role.Name).OrderBy(name => name).ToArrayAsync(TestContext.Current.CancellationToken);
            var tenantNames = await tenantDbContext.Tenants.Select(tenant => tenant.Name).OrderBy(name => name).ToArrayAsync(TestContext.Current.CancellationToken);

            roleNames.ShouldBe(["Administrators"]);
            tenantNames.ShouldBe(["Default"]);
        }
    }

    [Fact]
    public async Task WebHostComposition_ShouldExposeTenantResolutionStoreFromPersistence()
    {
        var databaseName = $"composition-resolution-store-{Guid.NewGuid():N}";
        var services = new ServiceCollection();
        services.UseDbContextOptions(options => options.UseInMemoryDatabase(databaseName));
        services.AddTestWebHost();

        await using var serviceProvider = services.BuildServiceProvider();
        var tenantId = Guid.NewGuid();

        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var tenantDbContext = scope.ServiceProvider.GetRequiredService<TenantManagementDbContext>();
            await tenantDbContext.Tenants.AddAsync(new TenantEntity(tenantId, "Acme", isActive: true), TestContext.Current.CancellationToken);
            await tenantDbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var verificationScope = serviceProvider.CreateAsyncScope())
        {
            var resolutionStore = verificationScope.ServiceProvider.GetRequiredService<ITenantResolutionStore>();

            var byId = await resolutionStore.FindByIdAsync(tenantId, TestContext.Current.CancellationToken);
            var byName = await resolutionStore.FindByNameAsync("acme", TestContext.Current.CancellationToken);

            byId.ShouldNotBeNull();
            byId.Id.ShouldBe(tenantId);
            byId.Name.ShouldBe("Acme");
            byId.IsActive.ShouldBeTrue();
            byName.ShouldNotBeNull();
            byName.Id.ShouldBe(tenantId);
        }
    }
}
