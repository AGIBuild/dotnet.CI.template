using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;
using ChengYuan.SettingManagement;
using ChengYuan.Settings;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class SettingManagementModuleTests
{
    [Fact]
    public void SettingManagementModule_ShouldLoadIntoTheServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddModule<SettingManagementTestModule>();
        services.AddInMemorySettingManagement();

        using var serviceProvider = services.BuildServiceProvider();

        var moduleCatalog = serviceProvider.GetRequiredService<ModuleCatalog>();
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(SettingManagementModule));
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(SettingsModule));

        serviceProvider.GetRequiredService<ISettingValueStore>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<ISettingValueManager>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<ISettingProvider>().ShouldNotBeNull();
    }

    [Fact]
    public async Task SettingManagement_ShouldApplyStoreBackedGlobalTenantAndUserValues()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var tenantId = Guid.NewGuid();
        const string userId = "alice";

        var services = new ServiceCollection();
        services.AddModule<SettingManagementTestModule>();
        services.AddInMemorySettingManagement();
        services.AddSingleton<ISettingDefinitionContributor>(new TestContributor(context =>
        {
            var group = context.AddGroup("workspace", "Workspace");
            group.AddSetting<int>("workspace.max-users", 10);
        }));

        using var serviceProvider = services.BuildServiceProvider();
        var settingProvider = serviceProvider.GetRequiredService<ISettingProvider>();
        var settingValueManager = serviceProvider.GetRequiredService<ISettingValueManager>();
        var currentTenant = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var currentUser = serviceProvider.GetRequiredService<ICurrentUserAccessor>();

        await settingValueManager.SetAsync(new SettingValueRecord("workspace.max-users", SettingScope.Global, 20), cancellationToken);
        (await settingProvider.GetAsync<int>("workspace.max-users", cancellationToken)).ShouldBe(20);

        await settingValueManager.SetAsync(new SettingValueRecord("workspace.max-users", SettingScope.Tenant, 30, tenantId), cancellationToken);

        using (currentTenant.Change(tenantId, "tenant-a"))
        {
            (await settingProvider.GetAsync<int>("workspace.max-users", cancellationToken)).ShouldBe(30);

            await settingValueManager.SetAsync(new SettingValueRecord("workspace.max-users", SettingScope.User, 40, userId: userId), cancellationToken);

            using (currentUser.Change(new CurrentUserInfo(userId, "Alice", true)))
            {
                (await settingProvider.GetAsync<int>("workspace.max-users", cancellationToken)).ShouldBe(40);
            }
        }
    }

    [Fact]
    public async Task SettingManagement_ShouldRemoveValuesAndFallbackToDefinitionDefault()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddModule<SettingManagementTestModule>();
        services.AddInMemorySettingManagement();
        services.AddSingleton<ISettingDefinitionContributor>(new TestContributor(context =>
        {
            var group = context.AddGroup("workspace", "Workspace");
            group.AddSetting<bool>("workspace.analytics.enabled", false);
        }));

        using var serviceProvider = services.BuildServiceProvider();
        var settingProvider = serviceProvider.GetRequiredService<ISettingProvider>();
        var settingValueManager = serviceProvider.GetRequiredService<ISettingValueManager>();

        await settingValueManager.SetAsync(new SettingValueRecord("workspace.analytics.enabled", SettingScope.Global, true), cancellationToken);
        (await settingProvider.GetAsync<bool>("workspace.analytics.enabled", cancellationToken)).ShouldBeTrue();

        await settingValueManager.RemoveAsync("workspace.analytics.enabled", SettingScope.Global, cancellationToken: cancellationToken);
        (await settingProvider.GetAsync<bool>("workspace.analytics.enabled", cancellationToken)).ShouldBeFalse();
    }

    [Fact]
    public async Task SettingManagement_ShouldReturnTypedValuesFromStore()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddModule<SettingManagementTestModule>();
        services.AddInMemorySettingManagement();
        services.AddSingleton<ISettingDefinitionContributor>(new TestContributor(context =>
        {
            var group = context.AddGroup("workspace", "Workspace");
            group.AddSetting<decimal>("workspace.storage-quota", 1m);
            group.AddSetting<string>("workspace.plan", "free");
        }));

        using var serviceProvider = services.BuildServiceProvider();
        var settingProvider = serviceProvider.GetRequiredService<ISettingProvider>();
        var settingValueManager = serviceProvider.GetRequiredService<ISettingValueManager>();

        await settingValueManager.SetAsync(new SettingValueRecord("workspace.storage-quota", SettingScope.Global, 12.5m), cancellationToken);
        await settingValueManager.SetAsync(new SettingValueRecord("workspace.plan", SettingScope.Global, "enterprise"), cancellationToken);

        (await settingProvider.GetAsync<decimal>("workspace.storage-quota", cancellationToken)).ShouldBe(12.5m);
        (await settingProvider.GetAsync<string>("workspace.plan", cancellationToken)).ShouldBe("enterprise");
    }

    [Fact]
    public async Task SettingValueStore_ShouldReturnStoredRecords()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var tenantId = Guid.NewGuid();

        var services = new ServiceCollection();
        services.AddModule<SettingManagementTestModule>();
        services.AddInMemorySettingManagement();

        using var serviceProvider = services.BuildServiceProvider();
        var settingValueStore = serviceProvider.GetRequiredService<ISettingValueStore>();

        await settingValueStore.SetAsync(new SettingValueRecord("workspace.title", SettingScope.Global, "Main"), cancellationToken);
        await settingValueStore.SetAsync(new SettingValueRecord("workspace.region", SettingScope.Tenant, "eu-west", tenantId), cancellationToken);
        await settingValueStore.SetAsync(new SettingValueRecord("workspace.theme", SettingScope.User, "light", userId: "alice"), cancellationToken);

        var records = await settingValueStore.GetListAsync(cancellationToken);
        records.Count.ShouldBe(3);
        records.Select(record => record.Name).ShouldContain("workspace.title");
        records.Select(record => record.Name).ShouldContain("workspace.region");
        records.Select(record => record.Name).ShouldContain("workspace.theme");
    }

    private sealed class TestContributor(Action<ISettingDefinitionContext> configure) : ISettingDefinitionContributor
    {
        public void Define(ISettingDefinitionContext context) => configure(context);
    }
}
