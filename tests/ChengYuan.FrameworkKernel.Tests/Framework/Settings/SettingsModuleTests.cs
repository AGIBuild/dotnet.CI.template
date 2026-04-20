using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;
using ChengYuan.Settings;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class SettingsModuleTests
{
    [Fact]
    public void SettingsModule_ShouldLoadIntoTheServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddModule<SettingsTestModule>();

        using var serviceProvider = services.BuildServiceProvider();

        var moduleCatalog = serviceProvider.GetRequiredService<ModuleCatalog>();
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(SettingsModule));
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(ExecutionContextModule));
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(MultiTenancyModule));

        serviceProvider.GetRequiredService<ISettingDefinitionManager>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<ISettingProvider>().ShouldNotBeNull();
    }

    [Fact]
    public void SettingDefinitionManager_ShouldRegisterAndFindDefinitions()
    {
        var services = new ServiceCollection();
        services.AddModule<SettingsTestModule>();
        services.AddSingleton<ISettingDefinitionContributor>(new TestContributor(context =>
        {
            var group = context.AddGroup("application", "Application");
            var setting = group.AddSetting<string>("app.name", "ChengYuan", "Application name");
            setting.Description = "The default application name.";
        }));

        using var serviceProvider = services.BuildServiceProvider();
        var manager = serviceProvider.GetRequiredService<ISettingDefinitionManager>();

        manager.IsDefined("app.name").ShouldBeTrue();
        var definition = manager.GetSetting("app.name");
        definition.DefaultValue.ShouldBe("ChengYuan");
        definition.DisplayName.ShouldBe("Application name");
        definition.Description.ShouldBe("The default application name.");
        definition.Group.ShouldNotBeNull();
        definition.Group!.Name.ShouldBe("application");
        manager.GetGroups().Count.ShouldBe(1);
        manager.GetAll().Count.ShouldBe(1);
    }

    [Fact]
    public async Task SettingProvider_ShouldFallbackToDefinitionDefault()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddModule<SettingsTestModule>();
        services.AddSingleton<ISettingDefinitionContributor>(new TestContributor(context =>
        {
            var group = context.AddGroup("app", "Application");
            group.AddSetting<string>("app.display-name", "ChengYuan");
        }));

        using var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetRequiredService<ISettingProvider>();

        (await provider.GetAsync<string>("app.display-name", cancellationToken)).ShouldBe("ChengYuan");
    }

    [Fact]
    public async Task SettingProvider_ShouldApplyGlobalThenTenantThenUserPrecedence()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddSingleton<ISettingValueProvider>(new TestGlobalSettingValueProvider(new Dictionary<string, object?>
        {
            ["app.timezone"] = "UTC"
        }));
        services.AddSingleton<ISettingValueProvider>(new TestTenantSettingValueProvider(new Dictionary<(Guid TenantId, string Name), object?>()));
        services.AddSingleton<ISettingValueProvider>(new TestUserSettingValueProvider(new Dictionary<(string UserId, string Name), object?>()));
        services.AddModule<SettingsTestModule>();
        services.AddSingleton<ISettingDefinitionContributor>(new TestContributor(context =>
        {
            var group = context.AddGroup("app", "Application");
            group.AddSetting<string>("app.timezone", "Asia/Shanghai");
        }));

        using var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetRequiredService<ISettingProvider>();
        var currentTenant = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var currentUser = serviceProvider.GetRequiredService<ICurrentUserAccessor>();

        (await provider.GetAsync<string>("app.timezone", cancellationToken)).ShouldBe("UTC");

        var tenantId = Guid.NewGuid();
        var userId = "alice";

        var tenantProvider = serviceProvider.GetServices<ISettingValueProvider>().OfType<TestTenantSettingValueProvider>().Single();
        tenantProvider.Values[(tenantId, "app.timezone")] = "Europe/Berlin";

        var userProvider = serviceProvider.GetServices<ISettingValueProvider>().OfType<TestUserSettingValueProvider>().Single();
        userProvider.Values[(userId, "app.timezone")] = "Australia/Sydney";

        using (currentTenant.Change(tenantId, "tenant-a"))
        {
            (await provider.GetAsync<string>("app.timezone", cancellationToken)).ShouldBe("Europe/Berlin");

            using (currentUser.Change(new CurrentUserInfo(userId, "Alice", true)))
            {
                (await provider.GetAsync<string>("app.timezone", cancellationToken)).ShouldBe("Australia/Sydney");
            }
        }
    }

    [Fact]
    public async Task SettingProvider_ShouldUseCurrentTenantContext()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var tenantId = Guid.NewGuid();
        var services = new ServiceCollection();
        services.AddSingleton<ISettingValueProvider>(new TestTenantSettingValueProvider(new Dictionary<(Guid TenantId, string Name), object?>
        {
            [(tenantId, "branding.logo-url")] = "tenant-a.svg"
        }));
        services.AddModule<SettingsTestModule>();
        services.AddSingleton<ISettingDefinitionContributor>(new TestContributor(context =>
        {
            var group = context.AddGroup("branding", "Branding");
            group.AddSetting<string>("branding.logo-url", "default.svg");
        }));

        using var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetRequiredService<ISettingProvider>();
        var currentTenant = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();

        using (currentTenant.Change(tenantId, "tenant-a"))
        {
            (await provider.GetAsync<string>("branding.logo-url", cancellationToken)).ShouldBe("tenant-a.svg");
        }
    }

    [Fact]
    public async Task SettingProvider_ShouldUseCurrentUserContext()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        const string userId = "alice";
        var services = new ServiceCollection();
        services.AddSingleton<ISettingValueProvider>(new TestUserSettingValueProvider(new Dictionary<(string UserId, string Name), object?>
        {
            [(userId, "ui.theme")] = "dark"
        }));
        services.AddModule<SettingsTestModule>();
        services.AddSingleton<ISettingDefinitionContributor>(new TestContributor(context =>
        {
            var group = context.AddGroup("ui", "UI");
            group.AddSetting<string>("ui.theme", "light");
        }));

        using var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetRequiredService<ISettingProvider>();
        var currentUser = serviceProvider.GetRequiredService<ICurrentUserAccessor>();

        using (currentUser.Change(new CurrentUserInfo(userId, "Alice", true)))
        {
            (await provider.GetAsync<string>("ui.theme", cancellationToken)).ShouldBe("dark");
        }
    }

    [Fact]
    public async Task SettingProvider_ShouldReturnTypedValues()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var workspaceId = Guid.NewGuid();
        var services = new ServiceCollection();
        services.AddSingleton<ISettingValueProvider>(new TestGlobalSettingValueProvider(new Dictionary<string, object?>
        {
            ["feature.enabled"] = true,
            ["workspace.max-users"] = 42,
            ["workspace.id"] = workspaceId.ToString()
        }));
        services.AddModule<SettingsTestModule>();
        services.AddSingleton<ISettingDefinitionContributor>(new TestContributor(context =>
        {
            var group = context.AddGroup("workspace", "Workspace");
            group.AddSetting<bool>("feature.enabled", false);
            group.AddSetting<int>("workspace.max-users", 10);
            group.AddSetting<Guid>("workspace.id", Guid.Empty);
        }));

        using var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetRequiredService<ISettingProvider>();

        (await provider.GetAsync<bool>("feature.enabled", cancellationToken)).ShouldBeTrue();
        (await provider.GetAsync<int>("workspace.max-users", cancellationToken)).ShouldBe(42);
        (await provider.GetAsync<Guid>("workspace.id", cancellationToken)).ShouldBe(workspaceId);
    }

    [Fact]
    public async Task SettingProvider_ShouldRejectTypeMismatchRequests()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddModule<SettingsTestModule>();
        services.AddSingleton<ISettingDefinitionContributor>(new TestContributor(context =>
        {
            var group = context.AddGroup("feature", "Feature");
            group.AddSetting<bool>("feature.enabled", true);
        }));

        using var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetRequiredService<ISettingProvider>();

        await Should.ThrowAsync<InvalidOperationException>(async () => _ = await provider.GetAsync<string>("feature.enabled", cancellationToken));
    }

    [Fact]
    public async Task AddInMemorySettings_ShouldResolveConfiguredValuesAcrossScopes()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var tenantId = Guid.NewGuid();
        const string userId = "alice";

        var services = new ServiceCollection();
        services.AddModule<SettingsTestModule>();
        services.AddSingleton<ISettingDefinitionContributor>(new TestContributor(context =>
        {
            var group = context.AddGroup("app", "Application");
            group.AddSetting<string>("app.timezone", "Asia/Shanghai");
            group.AddSetting<Guid>("workspace.id", Guid.Empty);
        }));
        services.AddInMemorySettings(builder => builder
            .SetGlobal("app.timezone", "UTC")
            .SetTenant("app.timezone", tenantId, "Europe/Berlin")
            .SetUser("app.timezone", userId, "Australia/Sydney")
            .SetGlobal("workspace.id", Guid.Parse("11111111-1111-1111-1111-111111111111").ToString()));

        using var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetRequiredService<ISettingProvider>();
        var currentTenant = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var currentUser = serviceProvider.GetRequiredService<ICurrentUserAccessor>();

        (await provider.GetAsync<string>("app.timezone", cancellationToken)).ShouldBe("UTC");
        (await provider.GetAsync<Guid>("workspace.id", cancellationToken)).ShouldBe(Guid.Parse("11111111-1111-1111-1111-111111111111"));

        using (currentTenant.Change(tenantId, "tenant-a"))
        {
            (await provider.GetAsync<string>("app.timezone", cancellationToken)).ShouldBe("Europe/Berlin");

            using (currentUser.Change(new CurrentUserInfo(userId, "Alice", true)))
            {
                (await provider.GetAsync<string>("app.timezone", cancellationToken)).ShouldBe("Australia/Sydney");
            }
        }
    }

    [DependsOn(typeof(SettingsModule))]
    private sealed class SettingsTestModule : FrameworkCoreModule
    {
    }

    private sealed class TestContributor(Action<ISettingDefinitionContext> configure) : ISettingDefinitionContributor
    {
        public void Define(ISettingDefinitionContext context) => configure(context);
    }

    private sealed class TestGlobalSettingValueProvider(Dictionary<string, object?> values) : ISettingValueProvider
    {
        public Dictionary<string, object?> Values { get; } = values;

        public string Name => "TestGlobal";

        public int Order => 110;

        public ValueTask<SettingValue?> GetOrNullAsync(SettingDefinition definition, SettingContext context, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(
                Values.TryGetValue(definition.Name, out var value)
                    ? new SettingValue(value, Name)
                    : null);
        }
    }

    private sealed class TestTenantSettingValueProvider(Dictionary<(Guid TenantId, string Name), object?> values) : ISettingValueProvider
    {
        public Dictionary<(Guid TenantId, string Name), object?> Values { get; } = values;

        public string Name => "TestTenant";

        public int Order => 210;

        public ValueTask<SettingValue?> GetOrNullAsync(SettingDefinition definition, SettingContext context, CancellationToken cancellationToken = default)
        {
            if (context.TenantId is not Guid tenantId)
            {
                return ValueTask.FromResult<SettingValue?>(null);
            }

            return ValueTask.FromResult(
                Values.TryGetValue((tenantId, definition.Name), out var value)
                    ? new SettingValue(value, Name)
                    : null);
        }
    }

    private sealed class TestUserSettingValueProvider(Dictionary<(string UserId, string Name), object?> values) : ISettingValueProvider
    {
        public Dictionary<(string UserId, string Name), object?> Values { get; } = values;

        public string Name => "TestUser";

        public int Order => 310;

        public ValueTask<SettingValue?> GetOrNullAsync(SettingDefinition definition, SettingContext context, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(context.UserId))
            {
                return ValueTask.FromResult<SettingValue?>(null);
            }

            return ValueTask.FromResult(
                Values.TryGetValue((context.UserId, definition.Name), out var value)
                    ? new SettingValue(value, Name)
                    : null);
        }
    }
}
