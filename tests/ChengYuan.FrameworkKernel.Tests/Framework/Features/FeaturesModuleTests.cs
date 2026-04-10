using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.Features;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class FeaturesModuleTests
{
    [Fact]
    public void FeaturesModule_ShouldLoadIntoTheServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddModule<FeaturesTestModule>();

        using var serviceProvider = services.BuildServiceProvider();

        var moduleCatalog = serviceProvider.GetRequiredService<ModuleCatalog>();
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(FeaturesModule));
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(ExecutionContextModule));
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(MultiTenancyModule));

        serviceProvider.GetRequiredService<IFeatureDefinitionManager>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<IFeatureChecker>().ShouldNotBeNull();
    }

    [Fact]
    public void FeatureDefinitionManager_ShouldRegisterAndFindDefinitions()
    {
        var services = new ServiceCollection();
        services.AddModule<FeaturesTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var manager = serviceProvider.GetRequiredService<IFeatureDefinitionManager>();

        manager.AddOrUpdate<bool>("workspace.analytics")
            .WithDefaultValue(false)
            .WithDisplayName("Workspace analytics")
            .WithDescription("Enables analytics capabilities.")
            .WithMetadata("group", "workspace");

        manager.IsDefined("workspace.analytics").ShouldBeTrue();
        var definition = manager.GetDefinition("workspace.analytics");
        definition.DefaultValue.ShouldBe(false);
        definition.DisplayName.ShouldBe("Workspace analytics");
        definition.Description.ShouldBe("Enables analytics capabilities.");
        definition.TryGetMetadata("group", out var group).ShouldBeTrue();
        group.ShouldBe("workspace");
        manager.GetAll().Count.ShouldBe(1);
    }

    [Fact]
    public async Task FeatureChecker_ShouldFallbackToDefinitionDefault()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddModule<FeaturesTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var manager = serviceProvider.GetRequiredService<IFeatureDefinitionManager>();
        var checker = serviceProvider.GetRequiredService<IFeatureChecker>();

        manager.AddOrUpdate<bool>("workspace.analytics").WithDefaultValue(true);

        (await checker.IsEnabledAsync("workspace.analytics", cancellationToken)).ShouldBeTrue();
    }

    [Fact]
    public async Task FeatureChecker_ShouldApplyGlobalThenTenantThenUserPrecedence()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddSingleton<IFeatureValueProvider>(new TestGlobalFeatureValueProvider(new Dictionary<string, object?>
        {
            ["workspace.max-users"] = 10
        }));
        services.AddSingleton<IFeatureValueProvider>(new TestTenantFeatureValueProvider(new Dictionary<(Guid TenantId, string Name), object?>()));
        services.AddSingleton<IFeatureValueProvider>(new TestUserFeatureValueProvider(new Dictionary<(string UserId, string Name), object?>()));
        services.AddModule<FeaturesTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var manager = serviceProvider.GetRequiredService<IFeatureDefinitionManager>();
        var checker = serviceProvider.GetRequiredService<IFeatureChecker>();
        var currentTenant = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var currentUser = serviceProvider.GetRequiredService<ICurrentUserAccessor>();

        manager.AddOrUpdate<int>("workspace.max-users").WithDefaultValue(5);

        (await checker.GetAsync<int>("workspace.max-users", cancellationToken)).ShouldBe(10);

        var tenantId = Guid.NewGuid();
        var userId = "alice";

        var tenantProvider = serviceProvider.GetServices<IFeatureValueProvider>().OfType<TestTenantFeatureValueProvider>().Single();
        tenantProvider.Values[(tenantId, "workspace.max-users")] = 20;

        var userProvider = serviceProvider.GetServices<IFeatureValueProvider>().OfType<TestUserFeatureValueProvider>().Single();
        userProvider.Values[(userId, "workspace.max-users")] = 30;

        using (currentTenant.Change(tenantId, "tenant-a"))
        {
            (await checker.GetAsync<int>("workspace.max-users", cancellationToken)).ShouldBe(20);

            using (currentUser.Change(new CurrentUserInfo(userId, "Alice", true)))
            {
                (await checker.GetAsync<int>("workspace.max-users", cancellationToken)).ShouldBe(30);
            }
        }
    }

    [Fact]
    public async Task FeatureChecker_ShouldReturnTypedValues()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddInMemoryFeatures(builder => builder
            .SetGlobal("workspace.analytics", true)
            .SetGlobal("workspace.max-users", 42)
            .SetGlobal("workspace.storage-quota", 12.5m)
            .SetGlobal("workspace.plan", "enterprise"));
        services.AddModule<FeaturesTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var manager = serviceProvider.GetRequiredService<IFeatureDefinitionManager>();
        var checker = serviceProvider.GetRequiredService<IFeatureChecker>();

        manager.AddOrUpdate<bool>("workspace.analytics").WithDefaultValue(false);
        manager.AddOrUpdate<int>("workspace.max-users").WithDefaultValue(5);
        manager.AddOrUpdate<decimal>("workspace.storage-quota").WithDefaultValue(1m);
        manager.AddOrUpdate<string>("workspace.plan").WithDefaultValue("free");

        (await checker.IsEnabledAsync("workspace.analytics", cancellationToken)).ShouldBeTrue();
        (await checker.GetAsync<int>("workspace.max-users", cancellationToken)).ShouldBe(42);
        (await checker.GetAsync<decimal>("workspace.storage-quota", cancellationToken)).ShouldBe(12.5m);
        (await checker.GetAsync<string>("workspace.plan", cancellationToken)).ShouldBe("enterprise");
    }

    [Fact]
    public async Task AddInMemoryFeatures_ShouldResolveConfiguredValuesAcrossScopes()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var tenantId = Guid.NewGuid();
        const string userId = "alice";

        var services = new ServiceCollection();
        services.AddModule<FeaturesTestModule>();
        services.AddInMemoryFeatures(builder => builder
            .SetGlobal("workspace.analytics", false)
            .SetTenant("workspace.analytics", tenantId, true)
            .SetUser("workspace.analytics", userId, false));

        using var serviceProvider = services.BuildServiceProvider();
        var manager = serviceProvider.GetRequiredService<IFeatureDefinitionManager>();
        var checker = serviceProvider.GetRequiredService<IFeatureChecker>();
        var currentTenant = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var currentUser = serviceProvider.GetRequiredService<ICurrentUserAccessor>();

        manager.AddOrUpdate<bool>("workspace.analytics").WithDefaultValue(true);

        (await checker.IsEnabledAsync("workspace.analytics", cancellationToken)).ShouldBeFalse();

        using (currentTenant.Change(tenantId, "tenant-a"))
        {
            (await checker.IsEnabledAsync("workspace.analytics", cancellationToken)).ShouldBeTrue();

            using (currentUser.Change(new CurrentUserInfo(userId, "Alice", true)))
            {
                (await checker.IsEnabledAsync("workspace.analytics", cancellationToken)).ShouldBeFalse();
            }
        }
    }

    [Fact]
    public async Task FeatureChecker_ShouldRejectTypeMismatchRequests()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddModule<FeaturesTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var manager = serviceProvider.GetRequiredService<IFeatureDefinitionManager>();
        var checker = serviceProvider.GetRequiredService<IFeatureChecker>();

        manager.AddOrUpdate<bool>("workspace.analytics").WithDefaultValue(true);

        await Should.ThrowAsync<InvalidOperationException>(async () => _ = await checker.GetAsync<string>("workspace.analytics", cancellationToken));
    }

    [DependsOn(typeof(FeaturesModule))]
    private sealed class FeaturesTestModule : ModuleBase
    {
    }

    private sealed class TestGlobalFeatureValueProvider(Dictionary<string, object?> values) : IFeatureValueProvider
    {
        public Dictionary<string, object?> Values { get; } = values;

        public string Name => "TestGlobal";

        public int Order => 110;

        public ValueTask<FeatureValue?> GetOrNullAsync(FeatureDefinition definition, FeatureContext context, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(
                Values.TryGetValue(definition.Name, out var value)
                    ? new FeatureValue(value, Name)
                    : null);
        }
    }

    private sealed class TestTenantFeatureValueProvider(Dictionary<(Guid TenantId, string Name), object?> values) : IFeatureValueProvider
    {
        public Dictionary<(Guid TenantId, string Name), object?> Values { get; } = values;

        public string Name => "TestTenant";

        public int Order => 210;

        public ValueTask<FeatureValue?> GetOrNullAsync(FeatureDefinition definition, FeatureContext context, CancellationToken cancellationToken = default)
        {
            if (context.TenantId is not Guid tenantId)
            {
                return ValueTask.FromResult<FeatureValue?>(null);
            }

            return ValueTask.FromResult(
                Values.TryGetValue((tenantId, definition.Name), out var value)
                    ? new FeatureValue(value, Name)
                    : null);
        }
    }

    private sealed class TestUserFeatureValueProvider(Dictionary<(string UserId, string Name), object?> values) : IFeatureValueProvider
    {
        public Dictionary<(string UserId, string Name), object?> Values { get; } = values;

        public string Name => "TestUser";

        public int Order => 310;

        public ValueTask<FeatureValue?> GetOrNullAsync(FeatureDefinition definition, FeatureContext context, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(context.UserId))
            {
                return ValueTask.FromResult<FeatureValue?>(null);
            }

            return ValueTask.FromResult(
                Values.TryGetValue((context.UserId, definition.Name), out var value)
                    ? new FeatureValue(value, Name)
                    : null);
        }
    }
}
