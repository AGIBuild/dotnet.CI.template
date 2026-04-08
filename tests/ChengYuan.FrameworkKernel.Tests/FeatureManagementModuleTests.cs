using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.FeatureManagement;
using ChengYuan.Features;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class FeatureManagementModuleTests
{
    [Fact]
    public void FeatureManagementModule_ShouldLoadIntoTheServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddModule<FeatureManagementTestModule>();
        services.AddInMemoryFeatureManagement();

        using var serviceProvider = services.BuildServiceProvider();

        var moduleCatalog = serviceProvider.GetRequiredService<ModuleCatalog>();
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(FeatureManagementModule));
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(FeaturesModule));

        serviceProvider.GetRequiredService<IFeatureValueStore>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<IFeatureValueManager>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<IFeatureChecker>().ShouldNotBeNull();
    }

    [Fact]
    public async Task FeatureManagement_ShouldApplyStoreBackedGlobalTenantAndUserValues()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var tenantId = Guid.NewGuid();
        const string userId = "alice";

        var services = new ServiceCollection();
        services.AddModule<FeatureManagementTestModule>();
        services.AddInMemoryFeatureManagement();

        using var serviceProvider = services.BuildServiceProvider();
        var definitionManager = serviceProvider.GetRequiredService<IFeatureDefinitionManager>();
        var featureChecker = serviceProvider.GetRequiredService<IFeatureChecker>();
        var featureValueManager = serviceProvider.GetRequiredService<IFeatureValueManager>();
        var currentTenant = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var currentUser = serviceProvider.GetRequiredService<ICurrentUserAccessor>();

        definitionManager.AddOrUpdate<int>("workspace.max-users").WithDefaultValue(10);

        await featureValueManager.SetAsync(new FeatureValueRecord("workspace.max-users", FeatureScope.Global, 20), cancellationToken);
        (await featureChecker.GetAsync<int>("workspace.max-users", cancellationToken)).ShouldBe(20);

        await featureValueManager.SetAsync(new FeatureValueRecord("workspace.max-users", FeatureScope.Tenant, 30, tenantId), cancellationToken);

        using (currentTenant.Change(tenantId, "tenant-a"))
        {
            (await featureChecker.GetAsync<int>("workspace.max-users", cancellationToken)).ShouldBe(30);

            await featureValueManager.SetAsync(new FeatureValueRecord("workspace.max-users", FeatureScope.User, 40, userId: userId), cancellationToken);

            using (currentUser.Change(new CurrentUserInfo(userId, "Alice", true)))
            {
                (await featureChecker.GetAsync<int>("workspace.max-users", cancellationToken)).ShouldBe(40);
            }
        }
    }

    [Fact]
    public async Task FeatureManagement_ShouldRemoveValuesAndFallbackToDefinitionDefault()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddModule<FeatureManagementTestModule>();
        services.AddInMemoryFeatureManagement();

        using var serviceProvider = services.BuildServiceProvider();
        var definitionManager = serviceProvider.GetRequiredService<IFeatureDefinitionManager>();
        var featureChecker = serviceProvider.GetRequiredService<IFeatureChecker>();
        var featureValueManager = serviceProvider.GetRequiredService<IFeatureValueManager>();

        definitionManager.AddOrUpdate<bool>("workspace.analytics.enabled").WithDefaultValue(false);

        await featureValueManager.SetAsync(new FeatureValueRecord("workspace.analytics.enabled", FeatureScope.Global, true), cancellationToken);
        (await featureChecker.IsEnabledAsync("workspace.analytics.enabled", cancellationToken)).ShouldBeTrue();

        await featureValueManager.RemoveAsync("workspace.analytics.enabled", FeatureScope.Global, cancellationToken: cancellationToken);
        (await featureChecker.IsEnabledAsync("workspace.analytics.enabled", cancellationToken)).ShouldBeFalse();
    }

    [Fact]
    public async Task FeatureManagement_ShouldReturnTypedValuesFromStore()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddModule<FeatureManagementTestModule>();
        services.AddInMemoryFeatureManagement();

        using var serviceProvider = services.BuildServiceProvider();
        var definitionManager = serviceProvider.GetRequiredService<IFeatureDefinitionManager>();
        var featureChecker = serviceProvider.GetRequiredService<IFeatureChecker>();
        var featureValueManager = serviceProvider.GetRequiredService<IFeatureValueManager>();

        definitionManager.AddOrUpdate<decimal>("workspace.storage-quota").WithDefaultValue(1m);
        definitionManager.AddOrUpdate<string>("workspace.plan").WithDefaultValue("free");

        await featureValueManager.SetAsync(new FeatureValueRecord("workspace.storage-quota", FeatureScope.Global, 12.5m), cancellationToken);
        await featureValueManager.SetAsync(new FeatureValueRecord("workspace.plan", FeatureScope.Global, "enterprise"), cancellationToken);

        (await featureChecker.GetAsync<decimal>("workspace.storage-quota", cancellationToken)).ShouldBe(12.5m);
        (await featureChecker.GetAsync<string>("workspace.plan", cancellationToken)).ShouldBe("enterprise");
    }

    [Fact]
    public async Task FeatureValueStore_ShouldReturnStoredRecords()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var tenantId = Guid.NewGuid();

        var services = new ServiceCollection();
        services.AddModule<FeatureManagementTestModule>();
        services.AddInMemoryFeatureManagement();

        using var serviceProvider = services.BuildServiceProvider();
        var featureValueStore = serviceProvider.GetRequiredService<IFeatureValueStore>();

        await featureValueStore.SetAsync(new FeatureValueRecord("workspace.title", FeatureScope.Global, "Main"), cancellationToken);
        await featureValueStore.SetAsync(new FeatureValueRecord("workspace.region", FeatureScope.Tenant, "eu-west", tenantId), cancellationToken);
        await featureValueStore.SetAsync(new FeatureValueRecord("workspace.theme", FeatureScope.User, "light", userId: "alice"), cancellationToken);

        var records = await featureValueStore.GetListAsync(cancellationToken);
        records.Count.ShouldBe(3);
        records.Select(record => record.Name).ShouldContain("workspace.title");
        records.Select(record => record.Name).ShouldContain("workspace.region");
        records.Select(record => record.Name).ShouldContain("workspace.theme");
    }
}
