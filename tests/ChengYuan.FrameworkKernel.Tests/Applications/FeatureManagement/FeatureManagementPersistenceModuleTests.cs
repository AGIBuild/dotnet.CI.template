using ChengYuan.Core.Modularity;
using ChengYuan.EntityFrameworkCore;
using ChengYuan.ExecutionContext;
using ChengYuan.FeatureManagement;
using ChengYuan.Features;
using ChengYuan.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class FeatureManagementPersistenceModuleTests
{
    [Fact]
    public void FeatureManagementPersistenceModule_ShouldLoadIntoTheServiceCollection()
    {
        var services = CreateServices();

        using var serviceProvider = services.BuildServiceProvider();

        var moduleCatalog = serviceProvider.GetRequiredService<ModuleCatalog>();
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(FeatureManagementPersistenceModule));
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(FeatureManagementModule));

        using var scope = serviceProvider.CreateScope();
        scope.ServiceProvider.GetRequiredService<FeatureManagementDbContext>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<IFeatureValueStore>().ShouldBeOfType<FeatureValueStore>();
        serviceProvider.GetRequiredService<IFeatureValueReader>().ShouldBeOfType<FeatureValueStore>();
        serviceProvider.GetRequiredService<IFeatureValueManager>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<IFeatureChecker>().ShouldNotBeNull();
    }

    [Fact]
    public async Task FeatureManagementPersistence_ShouldApplyStoreBackedGlobalTenantAndUserValues()
    {
        var services = CreateServices();

        await using var serviceProvider = services.BuildServiceProvider();
        var definitionManager = serviceProvider.GetRequiredService<IFeatureDefinitionManager>();
        var featureChecker = serviceProvider.GetRequiredService<IFeatureChecker>();
        var featureValueManager = serviceProvider.GetRequiredService<IFeatureValueManager>();
        var currentTenant = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var currentUser = serviceProvider.GetRequiredService<ICurrentUserAccessor>();
        var tenantId = Guid.NewGuid();

        definitionManager.AddOrUpdate<int>("workspace.max-users").WithDefaultValue(10);

        await featureValueManager.SetAsync(new FeatureValueRecord("workspace.max-users", FeatureScope.Global, 20), TestContext.Current.CancellationToken);
        (await featureChecker.GetAsync<int>("workspace.max-users", TestContext.Current.CancellationToken)).ShouldBe(20);

        await featureValueManager.SetAsync(new FeatureValueRecord("workspace.max-users", FeatureScope.Tenant, 30, tenantId), TestContext.Current.CancellationToken);

        using (currentTenant.Change(tenantId, "tenant-a"))
        {
            (await featureChecker.GetAsync<int>("workspace.max-users", TestContext.Current.CancellationToken)).ShouldBe(30);

            await featureValueManager.SetAsync(new FeatureValueRecord("workspace.max-users", FeatureScope.User, 40, userId: "alice"), TestContext.Current.CancellationToken);

            using (currentUser.Change(new CurrentUserInfo("alice", "Alice", true)))
            {
                (await featureChecker.GetAsync<int>("workspace.max-users", TestContext.Current.CancellationToken)).ShouldBe(40);
            }
        }
    }

    [Fact]
    public async Task FeatureManagementPersistence_ShouldRemoveValuesAndFallbackToDefinitionDefault()
    {
        var services = CreateServices();

        await using var serviceProvider = services.BuildServiceProvider();
        var definitionManager = serviceProvider.GetRequiredService<IFeatureDefinitionManager>();
        var featureChecker = serviceProvider.GetRequiredService<IFeatureChecker>();
        var featureValueManager = serviceProvider.GetRequiredService<IFeatureValueManager>();

        definitionManager.AddOrUpdate<bool>("workspace.analytics.enabled").WithDefaultValue(false);

        await featureValueManager.SetAsync(new FeatureValueRecord("workspace.analytics.enabled", FeatureScope.Global, true), TestContext.Current.CancellationToken);
        (await featureChecker.IsEnabledAsync("workspace.analytics.enabled", TestContext.Current.CancellationToken)).ShouldBeTrue();

        await featureValueManager.RemoveAsync("workspace.analytics.enabled", FeatureScope.Global, cancellationToken: TestContext.Current.CancellationToken);
        (await featureChecker.IsEnabledAsync("workspace.analytics.enabled", TestContext.Current.CancellationToken)).ShouldBeFalse();
    }

    [Fact]
    public async Task FeatureManagementPersistence_ShouldReturnTypedValuesFromStore()
    {
        var services = CreateServices();

        await using var serviceProvider = services.BuildServiceProvider();
        var definitionManager = serviceProvider.GetRequiredService<IFeatureDefinitionManager>();
        var featureChecker = serviceProvider.GetRequiredService<IFeatureChecker>();
        var featureValueManager = serviceProvider.GetRequiredService<IFeatureValueManager>();

        definitionManager.AddOrUpdate<decimal>("workspace.storage-quota").WithDefaultValue(1m);
        definitionManager.AddOrUpdate<string>("workspace.plan").WithDefaultValue("free");

        await featureValueManager.SetAsync(new FeatureValueRecord("workspace.storage-quota", FeatureScope.Global, 12.5m), TestContext.Current.CancellationToken);
        await featureValueManager.SetAsync(new FeatureValueRecord("workspace.plan", FeatureScope.Global, "enterprise"), TestContext.Current.CancellationToken);

        (await featureChecker.GetAsync<decimal>("workspace.storage-quota", TestContext.Current.CancellationToken)).ShouldBe(12.5m);
        (await featureChecker.GetAsync<string>("workspace.plan", TestContext.Current.CancellationToken)).ShouldBe("enterprise");
    }

    [Fact]
    public async Task FeatureManagementPersistence_ShouldReturnStoredRecords()
    {
        var services = CreateServices();

        await using var serviceProvider = services.BuildServiceProvider();
        var featureValueStore = serviceProvider.GetRequiredService<IFeatureValueStore>();

        await featureValueStore.SetAsync(new FeatureValueRecord("workspace.title", FeatureScope.Global, "Main"), TestContext.Current.CancellationToken);
        await featureValueStore.SetAsync(new FeatureValueRecord("workspace.region", FeatureScope.Tenant, "eu-west", Guid.NewGuid()), TestContext.Current.CancellationToken);
        await featureValueStore.SetAsync(new FeatureValueRecord("workspace.theme", FeatureScope.User, "light", userId: "alice"), TestContext.Current.CancellationToken);

        var records = await featureValueStore.GetListAsync(TestContext.Current.CancellationToken);
        records.Count.ShouldBe(3);
        records.Select(record => record.Name).ShouldContain("workspace.title");
        records.Select(record => record.Name).ShouldContain("workspace.region");
        records.Select(record => record.Name).ShouldContain("workspace.theme");
    }

    private static ServiceCollection CreateServices()
    {
        var databaseName = $"feature-management-{Guid.NewGuid():N}";
        var services = new ServiceCollection();
        services.AddModule<FeatureManagementPersistenceTestModule>();
        services.UseDbContextOptions(options =>
            options.UseInMemoryDatabase(databaseName));

        return services;
    }
}
