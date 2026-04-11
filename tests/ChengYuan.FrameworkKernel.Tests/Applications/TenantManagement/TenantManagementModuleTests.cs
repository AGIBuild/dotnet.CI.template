using ChengYuan.Core.Modularity;
using ChengYuan.MultiTenancy;
using ChengYuan.TenantManagement;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class TenantManagementModuleTests
{
    [Fact]
    public void TenantManagementModule_ShouldLoadIntoTheServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddModule<TenantManagementTestModule>();
        services.AddInMemoryTenantManagement();

        using var serviceProvider = services.BuildServiceProvider();

        var moduleCatalog = serviceProvider.GetRequiredService<ModuleCatalog>();
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(TenantManagementModule));
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(MultiTenancyModule));

        serviceProvider.GetRequiredService<ITenantStore>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<ITenantReader>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<ITenantManager>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<ITenantResolutionStore>().ShouldNotBeNull();
    }

    [Fact]
    public async Task TenantManagement_ShouldCreateAndFindTenants()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddModule<TenantManagementTestModule>();
        services.AddInMemoryTenantManagement();

        using var serviceProvider = services.BuildServiceProvider();
        var tenantManager = serviceProvider.GetRequiredService<ITenantManager>();
        var tenantReader = serviceProvider.GetRequiredService<ITenantReader>();

        var tenant = await tenantManager.CreateAsync("alpha", cancellationToken: cancellationToken);

        (await tenantReader.FindByIdAsync(tenant.Id, cancellationToken)).ShouldNotBeNull();
        (await tenantReader.FindByNameAsync("ALPHA", cancellationToken)).ShouldNotBeNull();
    }

    [Fact]
    public async Task TenantManagement_ShouldUpdateExistingTenant()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddModule<TenantManagementTestModule>();
        services.AddInMemoryTenantManagement();

        using var serviceProvider = services.BuildServiceProvider();
        var tenantManager = serviceProvider.GetRequiredService<ITenantManager>();
        var tenantReader = serviceProvider.GetRequiredService<ITenantReader>();

        var tenant = await tenantManager.CreateAsync("alpha", cancellationToken: cancellationToken);
        await tenantManager.SetAsync(new TenantRecord(tenant.Id, "beta", isActive: false), cancellationToken);

        var updatedTenant = await tenantReader.FindByIdAsync(tenant.Id, cancellationToken);
        updatedTenant.ShouldNotBeNull();
        updatedTenant.Name.ShouldBe("beta");
        updatedTenant.IsActive.ShouldBeFalse();
        (await tenantReader.FindByNameAsync("alpha", cancellationToken)).ShouldBeNull();
    }

    [Fact]
    public async Task TenantManagement_ShouldPreventDuplicateTenantNames()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddModule<TenantManagementTestModule>();
        services.AddInMemoryTenantManagement();

        using var serviceProvider = services.BuildServiceProvider();
        var tenantManager = serviceProvider.GetRequiredService<ITenantManager>();

        await tenantManager.CreateAsync("alpha", cancellationToken: cancellationToken);

        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await tenantManager.CreateAsync("ALPHA", cancellationToken: cancellationToken));
    }

    [Fact]
    public async Task TenantManagement_ShouldRemoveTenant()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddModule<TenantManagementTestModule>();
        services.AddInMemoryTenantManagement();

        using var serviceProvider = services.BuildServiceProvider();
        var tenantManager = serviceProvider.GetRequiredService<ITenantManager>();
        var tenantReader = serviceProvider.GetRequiredService<ITenantReader>();

        var tenant = await tenantManager.CreateAsync("alpha", cancellationToken: cancellationToken);
        await tenantManager.RemoveAsync(tenant.Id, cancellationToken);

        (await tenantReader.FindByIdAsync(tenant.Id, cancellationToken)).ShouldBeNull();
        (await tenantReader.FindByNameAsync("alpha", cancellationToken)).ShouldBeNull();
    }

    [Fact]
    public async Task TenantStore_ShouldReturnSortedTenants()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddModule<TenantManagementTestModule>();
        services.AddInMemoryTenantManagement();

        using var serviceProvider = services.BuildServiceProvider();
        var tenantStore = serviceProvider.GetRequiredService<ITenantStore>();

        await tenantStore.SetAsync(new TenantRecord(Guid.NewGuid(), "gamma"), cancellationToken);
        await tenantStore.SetAsync(new TenantRecord(Guid.NewGuid(), "alpha"), cancellationToken);
        await tenantStore.SetAsync(new TenantRecord(Guid.NewGuid(), "beta"), cancellationToken);

        var tenants = await tenantStore.GetListAsync(cancellationToken);
        tenants.Select(tenant => tenant.Name).ShouldBe(["alpha", "beta", "gamma"]);
    }

    [Fact]
    public async Task TenantManagement_ShouldExposeResolutionStoreFromInMemoryCatalog()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddModule<TenantManagementTestModule>();
        services.AddInMemoryTenantManagement();

        using var serviceProvider = services.BuildServiceProvider();
        var tenantManager = serviceProvider.GetRequiredService<ITenantManager>();
        var resolutionStore = serviceProvider.GetRequiredService<ITenantResolutionStore>();

        var tenant = await tenantManager.CreateAsync("alpha", cancellationToken: cancellationToken);

        var byId = await resolutionStore.FindByIdAsync(tenant.Id, cancellationToken);
        var byName = await resolutionStore.FindByNameAsync("ALPHA", cancellationToken);

        byId.ShouldNotBeNull();
        byId.Id.ShouldBe(tenant.Id);
        byId.Name.ShouldBe("alpha");
        byId.IsActive.ShouldBeTrue();
        byName.ShouldNotBeNull();
        byName.Id.ShouldBe(tenant.Id);
    }
}
