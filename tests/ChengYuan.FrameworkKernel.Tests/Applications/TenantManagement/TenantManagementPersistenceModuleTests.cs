using ChengYuan.Core.Data;
using ChengYuan.Core.Modularity;
using ChengYuan.EntityFrameworkCore;
using ChengYuan.TenantManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class TenantManagementPersistenceModuleTests
{
    [Fact]
    public void TenantManagementPersistenceModule_ShouldLoadIntoTheServiceCollection()
    {
        var services = CreateServices();

        using var serviceProvider = services.BuildServiceProvider();

        var moduleCatalog = serviceProvider.GetRequiredService<ModuleCatalog>();
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(TenantManagementPersistenceModule));
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(TenantManagementModule));

        using var scope = serviceProvider.CreateScope();
        scope.ServiceProvider.GetRequiredService<TenantManagementDbContext>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<ITenantStore>().ShouldBeOfType<TenantStore>();
        scope.ServiceProvider.GetRequiredService<ITenantReader>().ShouldBeOfType<TenantStore>();
        scope.ServiceProvider.GetRequiredService<IUnitOfWork>().ShouldNotBeNull();
    }

    [Fact]
    public async Task TenantManagementPersistence_ShouldCreateAndFindTenants()
    {
        var services = CreateServices();

        await using var serviceProvider = services.BuildServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var tenantManager = scope.ServiceProvider.GetRequiredService<ITenantManager>();
        var tenantReader = scope.ServiceProvider.GetRequiredService<ITenantReader>();

        var tenant = await tenantManager.CreateAsync("alpha", cancellationToken: TestContext.Current.CancellationToken);

        (await tenantReader.FindByIdAsync(tenant.Id, TestContext.Current.CancellationToken)).ShouldNotBeNull();
        (await tenantReader.FindByNameAsync("ALPHA", TestContext.Current.CancellationToken)).ShouldNotBeNull();
    }

    [Fact]
    public async Task TenantManagementPersistence_ShouldUpdateExistingTenant()
    {
        var services = CreateServices();

        await using var serviceProvider = services.BuildServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var tenantManager = scope.ServiceProvider.GetRequiredService<ITenantManager>();
        var tenantReader = scope.ServiceProvider.GetRequiredService<ITenantReader>();

        var tenant = await tenantManager.CreateAsync("alpha", cancellationToken: TestContext.Current.CancellationToken);
        await tenantManager.SetAsync(new TenantRecord(tenant.Id, "beta", isActive: false), TestContext.Current.CancellationToken);

        var updatedTenant = await tenantReader.FindByIdAsync(tenant.Id, TestContext.Current.CancellationToken);
        updatedTenant.ShouldNotBeNull();
        updatedTenant.Name.ShouldBe("beta");
        updatedTenant.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task TenantManagementPersistence_ShouldPreventDuplicateTenantNames()
    {
        var services = CreateServices();

        await using var serviceProvider = services.BuildServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var tenantManager = scope.ServiceProvider.GetRequiredService<ITenantManager>();

        await tenantManager.CreateAsync("alpha", cancellationToken: TestContext.Current.CancellationToken);

        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await tenantManager.CreateAsync("ALPHA", cancellationToken: TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TenantManagementPersistence_ShouldSoftDeleteTenantRows()
    {
        var services = CreateServices();

        await using var serviceProvider = services.BuildServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var tenantManager = scope.ServiceProvider.GetRequiredService<ITenantManager>();
        var tenantReader = scope.ServiceProvider.GetRequiredService<ITenantReader>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TenantManagementDbContext>();

        var tenant = await tenantManager.CreateAsync("alpha", cancellationToken: TestContext.Current.CancellationToken);
        await tenantManager.RemoveAsync(tenant.Id, TestContext.Current.CancellationToken);

        (await tenantReader.FindByIdAsync(tenant.Id, TestContext.Current.CancellationToken)).ShouldBeNull();

        var deletedTenant = await dbContext.Tenants.SingleAsync(entity => entity.Id == tenant.Id, TestContext.Current.CancellationToken);
        deletedTenant.IsDeleted.ShouldBeTrue();
        deletedTenant.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task TenantManagementPersistence_ShouldReturnSortedTenants()
    {
        var services = CreateServices();

        await using var serviceProvider = services.BuildServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var tenantStore = scope.ServiceProvider.GetRequiredService<ITenantStore>();

        await tenantStore.SetAsync(new TenantRecord(Guid.NewGuid(), "gamma"), TestContext.Current.CancellationToken);
        await tenantStore.SetAsync(new TenantRecord(Guid.NewGuid(), "alpha"), TestContext.Current.CancellationToken);
        await tenantStore.SetAsync(new TenantRecord(Guid.NewGuid(), "beta"), TestContext.Current.CancellationToken);

        var tenants = await tenantStore.GetListAsync(TestContext.Current.CancellationToken);
        tenants.Select(tenant => tenant.Name).ShouldBe(["alpha", "beta", "gamma"]);
    }

    private static ServiceCollection CreateServices()
    {
        var databaseName = $"tenant-management-{Guid.NewGuid():N}";
        var services = new ServiceCollection();
        services.AddModule<TenantManagementPersistenceTestModule>();
        services.UseDbContextOptions(options =>
            options.UseInMemoryDatabase(databaseName));

        return services;
    }
}
