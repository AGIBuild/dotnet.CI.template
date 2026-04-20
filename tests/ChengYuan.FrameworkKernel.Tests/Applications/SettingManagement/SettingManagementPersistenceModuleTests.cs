using ChengYuan.Core.Modularity;
using ChengYuan.EntityFrameworkCore;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;
using ChengYuan.SettingManagement;
using ChengYuan.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class SettingManagementPersistenceModuleTests
{
    [Fact]
    public void SettingManagementPersistenceModule_ShouldLoadIntoTheServiceCollection()
    {
        var services = CreateServices();

        using var serviceProvider = services.BuildServiceProvider();

        var moduleCatalog = serviceProvider.GetRequiredService<ModuleCatalog>();
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(SettingManagementPersistenceModule));
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(SettingManagementModule));

        using var scope = serviceProvider.CreateScope();
        scope.ServiceProvider.GetRequiredService<SettingManagementDbContext>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<ISettingValueStore>().ShouldBeOfType<SettingValueStore>();
        serviceProvider.GetRequiredService<ISettingValueReader>().ShouldBeOfType<SettingValueStore>();
        serviceProvider.GetRequiredService<ISettingValueManager>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<ISettingProvider>().ShouldNotBeNull();
    }

    [Fact]
    public async Task SettingManagementPersistence_ShouldApplyStoreBackedGlobalTenantAndUserValues()
    {
        var services = CreateServices();
        services.AddSingleton<ISettingDefinitionContributor>(new TestContributor(context =>
        {
            var group = context.AddGroup("workspace", "Workspace");
            group.AddSetting<int>("workspace.max-users", 10);
        }));

        await using var serviceProvider = services.BuildServiceProvider();
        var settingProvider = serviceProvider.GetRequiredService<ISettingProvider>();
        var settingValueManager = serviceProvider.GetRequiredService<ISettingValueManager>();
        var currentTenant = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var currentUser = serviceProvider.GetRequiredService<ICurrentUserAccessor>();
        var tenantId = Guid.NewGuid();

        await settingValueManager.SetAsync(new SettingValueRecord("workspace.max-users", SettingScope.Global, 20), TestContext.Current.CancellationToken);
        (await settingProvider.GetAsync<int>("workspace.max-users", TestContext.Current.CancellationToken)).ShouldBe(20);

        await settingValueManager.SetAsync(new SettingValueRecord("workspace.max-users", SettingScope.Tenant, 30, tenantId), TestContext.Current.CancellationToken);

        using (currentTenant.Change(tenantId, "tenant-a"))
        {
            (await settingProvider.GetAsync<int>("workspace.max-users", TestContext.Current.CancellationToken)).ShouldBe(30);

            await settingValueManager.SetAsync(new SettingValueRecord("workspace.max-users", SettingScope.User, 40, userId: "alice"), TestContext.Current.CancellationToken);

            using (currentUser.Change(new CurrentUserInfo("alice", "Alice", true)))
            {
                (await settingProvider.GetAsync<int>("workspace.max-users", TestContext.Current.CancellationToken)).ShouldBe(40);
            }
        }
    }

    [Fact]
    public async Task SettingManagementPersistence_ShouldRemoveValuesAndFallbackToDefinitionDefault()
    {
        var services = CreateServices();
        services.AddSingleton<ISettingDefinitionContributor>(new TestContributor(context =>
        {
            var group = context.AddGroup("workspace", "Workspace");
            group.AddSetting<bool>("workspace.analytics.enabled", false);
        }));

        await using var serviceProvider = services.BuildServiceProvider();
        var settingProvider = serviceProvider.GetRequiredService<ISettingProvider>();
        var settingValueManager = serviceProvider.GetRequiredService<ISettingValueManager>();

        await settingValueManager.SetAsync(new SettingValueRecord("workspace.analytics.enabled", SettingScope.Global, true), TestContext.Current.CancellationToken);
        (await settingProvider.GetAsync<bool>("workspace.analytics.enabled", TestContext.Current.CancellationToken)).ShouldBeTrue();

        await settingValueManager.RemoveAsync("workspace.analytics.enabled", SettingScope.Global, cancellationToken: TestContext.Current.CancellationToken);
        (await settingProvider.GetAsync<bool>("workspace.analytics.enabled", TestContext.Current.CancellationToken)).ShouldBeFalse();
    }

    [Fact]
    public async Task SettingManagementPersistence_ShouldReturnTypedValuesFromStore()
    {
        var services = CreateServices();
        services.AddSingleton<ISettingDefinitionContributor>(new TestContributor(context =>
        {
            var group = context.AddGroup("workspace", "Workspace");
            group.AddSetting<decimal>("workspace.storage-quota", 1m);
            group.AddSetting<string>("workspace.plan", "free");
        }));

        await using var serviceProvider = services.BuildServiceProvider();
        var settingProvider = serviceProvider.GetRequiredService<ISettingProvider>();
        var settingValueManager = serviceProvider.GetRequiredService<ISettingValueManager>();

        await settingValueManager.SetAsync(new SettingValueRecord("workspace.storage-quota", SettingScope.Global, 12.5m), TestContext.Current.CancellationToken);
        await settingValueManager.SetAsync(new SettingValueRecord("workspace.plan", SettingScope.Global, "enterprise"), TestContext.Current.CancellationToken);

        (await settingProvider.GetAsync<decimal>("workspace.storage-quota", TestContext.Current.CancellationToken)).ShouldBe(12.5m);
        (await settingProvider.GetAsync<string>("workspace.plan", TestContext.Current.CancellationToken)).ShouldBe("enterprise");
    }

    [Fact]
    public async Task SettingManagementPersistence_ShouldReturnStoredRecords()
    {
        var services = CreateServices();

        await using var serviceProvider = services.BuildServiceProvider();
        var settingValueStore = serviceProvider.GetRequiredService<ISettingValueStore>();

        await settingValueStore.SetAsync(new SettingValueRecord("workspace.title", SettingScope.Global, "Main"), TestContext.Current.CancellationToken);
        await settingValueStore.SetAsync(new SettingValueRecord("workspace.region", SettingScope.Tenant, "eu-west", Guid.NewGuid()), TestContext.Current.CancellationToken);
        await settingValueStore.SetAsync(new SettingValueRecord("workspace.theme", SettingScope.User, "light", userId: "alice"), TestContext.Current.CancellationToken);

        var records = await settingValueStore.GetListAsync(TestContext.Current.CancellationToken);
        records.Count.ShouldBe(3);
        records.Select(record => record.Name).ShouldContain("workspace.title");
        records.Select(record => record.Name).ShouldContain("workspace.region");
        records.Select(record => record.Name).ShouldContain("workspace.theme");
    }

    private static ServiceCollection CreateServices()
    {
        var databaseName = $"setting-management-{Guid.NewGuid():N}";
        var services = new ServiceCollection();
        services.AddModule<SettingManagementPersistenceTestModule>();
        services.UseDbContextOptions(options =>
            options.UseInMemoryDatabase(databaseName));

        return services;
    }

    private sealed class TestContributor(Action<ISettingDefinitionContext> configure) : ISettingDefinitionContributor
    {
        public void Define(ISettingDefinitionContext context) => configure(context);
    }
}
