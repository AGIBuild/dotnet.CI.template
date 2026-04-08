using ChengYuan.Authorization;
using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class AuthorizationModuleTests
{
    [Fact]
    public void AuthorizationModule_ShouldLoadIntoTheServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddModule<AuthorizationTestModule>();

        using var serviceProvider = services.BuildServiceProvider();

        var moduleCatalog = serviceProvider.GetRequiredService<ModuleCatalog>();
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(AuthorizationModule));
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(ExecutionContextModule));
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(MultiTenancyModule));

        serviceProvider.GetRequiredService<IPermissionDefinitionManager>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<IPermissionChecker>().ShouldNotBeNull();
    }

    [Fact]
    public void PermissionDefinitionManager_ShouldRegisterAndFindDefinitions()
    {
        var services = new ServiceCollection();
        services.AddModule<AuthorizationTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var manager = serviceProvider.GetRequiredService<IPermissionDefinitionManager>();

        manager.AddOrUpdate("workspace.members.delete")
            .WithDefaultGrant(false)
            .WithDisplayName("Delete workspace members")
            .WithDescription("Allows deleting members from the current workspace.")
            .WithMetadata("group", "workspace.members");

        manager.IsDefined("workspace.members.delete").ShouldBeTrue();
        var definition = manager.GetDefinition("workspace.members.delete");
        definition.DefaultGranted.ShouldBeFalse();
        definition.DisplayName.ShouldBe("Delete workspace members");
        definition.Description.ShouldBe("Allows deleting members from the current workspace.");
        definition.TryGetMetadata("group", out var group).ShouldBeTrue();
        group.ShouldBe("workspace.members");
        manager.GetAll().Count.ShouldBe(1);
    }

    [Fact]
    public async Task PermissionChecker_ShouldFallbackToDefinitionDefault()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddModule<AuthorizationTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var manager = serviceProvider.GetRequiredService<IPermissionDefinitionManager>();
        var checker = serviceProvider.GetRequiredService<IPermissionChecker>();

        manager.AddOrUpdate("workspace.analytics.view").WithDefaultGrant(true);

        (await checker.IsGrantedAsync("workspace.analytics.view", cancellationToken)).ShouldBeTrue();
    }

    [Fact]
    public async Task PermissionChecker_ShouldApplyGlobalThenTenantThenUserPrecedence()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddSingleton<IPermissionGrantProvider>(new TestGlobalPermissionGrantProvider(new Dictionary<string, bool>
        {
            ["workspace.members.delete"] = true
        }));
        services.AddSingleton<IPermissionGrantProvider>(new TestTenantPermissionGrantProvider(new Dictionary<(Guid TenantId, string Name), bool>()));
        services.AddSingleton<IPermissionGrantProvider>(new TestUserPermissionGrantProvider(new Dictionary<(string UserId, string Name), bool>()));
        services.AddModule<AuthorizationTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var manager = serviceProvider.GetRequiredService<IPermissionDefinitionManager>();
        var checker = serviceProvider.GetRequiredService<IPermissionChecker>();
        var currentTenant = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var currentUser = serviceProvider.GetRequiredService<ICurrentUserAccessor>();

        manager.AddOrUpdate("workspace.members.delete").WithDefaultGrant(false);

        (await checker.IsGrantedAsync("workspace.members.delete", cancellationToken)).ShouldBeTrue();

        var tenantId = Guid.NewGuid();
        const string userId = "alice";

        var tenantProvider = serviceProvider.GetServices<IPermissionGrantProvider>().OfType<TestTenantPermissionGrantProvider>().Single();
        tenantProvider.Values[(tenantId, "workspace.members.delete")] = false;

        var userProvider = serviceProvider.GetServices<IPermissionGrantProvider>().OfType<TestUserPermissionGrantProvider>().Single();
        userProvider.Values[(userId, "workspace.members.delete")] = true;

        using (currentTenant.Change(tenantId, "tenant-a"))
        {
            (await checker.IsGrantedAsync("workspace.members.delete", cancellationToken)).ShouldBeFalse();

            using (currentUser.Change(new CurrentUserInfo(userId, "Alice", true)))
            {
                (await checker.IsGrantedAsync("workspace.members.delete", cancellationToken)).ShouldBeTrue();
            }
        }
    }

    [Fact]
    public async Task PermissionChecker_ShouldUseCurrentAuthenticationState()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddSingleton<IPermissionGrantProvider>(new AuthenticatedUserPermissionGrantProvider());
        services.AddModule<AuthorizationTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var manager = serviceProvider.GetRequiredService<IPermissionDefinitionManager>();
        var checker = serviceProvider.GetRequiredService<IPermissionChecker>();
        var currentUser = serviceProvider.GetRequiredService<ICurrentUserAccessor>();

        manager.AddOrUpdate("workspace.analytics.view").WithDefaultGrant(false);

        (await checker.IsGrantedAsync("workspace.analytics.view", cancellationToken)).ShouldBeFalse();

        using (currentUser.Change(new CurrentUserInfo("alice", "Alice", true)))
        {
            (await checker.IsGrantedAsync("workspace.analytics.view", cancellationToken)).ShouldBeTrue();
        }
    }

    [Fact]
    public async Task AddInMemoryPermissions_ShouldResolveConfiguredGrantsAcrossScopes()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var tenantId = Guid.NewGuid();
        const string userId = "alice";

        var services = new ServiceCollection();
        services.AddModule<AuthorizationTestModule>();
        services.AddInMemoryPermissions(builder => builder
            .SetGlobal("workspace.members.delete", false)
            .SetTenant("workspace.members.delete", tenantId, true)
            .SetUser("workspace.members.delete", userId, false));

        using var serviceProvider = services.BuildServiceProvider();
        var manager = serviceProvider.GetRequiredService<IPermissionDefinitionManager>();
        var checker = serviceProvider.GetRequiredService<IPermissionChecker>();
        var currentTenant = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var currentUser = serviceProvider.GetRequiredService<ICurrentUserAccessor>();

        manager.AddOrUpdate("workspace.members.delete").WithDefaultGrant(true);

        (await checker.IsGrantedAsync("workspace.members.delete", cancellationToken)).ShouldBeFalse();

        using (currentTenant.Change(tenantId, "tenant-a"))
        {
            (await checker.IsGrantedAsync("workspace.members.delete", cancellationToken)).ShouldBeTrue();

            using (currentUser.Change(new CurrentUserInfo(userId, "Alice", true)))
            {
                (await checker.IsGrantedAsync("workspace.members.delete", cancellationToken)).ShouldBeFalse();
            }
        }
    }

    [Fact]
    public async Task PermissionChecker_ShouldThrowWhenPermissionIsUndefined()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddModule<AuthorizationTestModule>();

        using var serviceProvider = services.BuildServiceProvider();
        var checker = serviceProvider.GetRequiredService<IPermissionChecker>();

        await Should.ThrowAsync<InvalidOperationException>(async () => _ = await checker.IsGrantedAsync("workspace.unknown", cancellationToken));
    }

    [DependsOn(typeof(AuthorizationModule))]
    private sealed class AuthorizationTestModule : ModuleBase
    {
    }

    private sealed class TestGlobalPermissionGrantProvider(Dictionary<string, bool> values) : IPermissionGrantProvider
    {
        public Dictionary<string, bool> Values { get; } = values;

        public string Name => "TestGlobal";

        public int Order => 110;

        public ValueTask<PermissionGrant?> GetOrNullAsync(PermissionDefinition definition, PermissionContext context, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(
                Values.TryGetValue(definition.Name, out var isGranted)
                    ? new PermissionGrant(isGranted, Name)
                    : null);
        }
    }

    private sealed class TestTenantPermissionGrantProvider(Dictionary<(Guid TenantId, string Name), bool> values) : IPermissionGrantProvider
    {
        public Dictionary<(Guid TenantId, string Name), bool> Values { get; } = values;

        public string Name => "TestTenant";

        public int Order => 210;

        public ValueTask<PermissionGrant?> GetOrNullAsync(PermissionDefinition definition, PermissionContext context, CancellationToken cancellationToken = default)
        {
            if (context.TenantId is not Guid tenantId)
            {
                return ValueTask.FromResult<PermissionGrant?>(null);
            }

            return ValueTask.FromResult(
                Values.TryGetValue((tenantId, definition.Name), out var isGranted)
                    ? new PermissionGrant(isGranted, Name)
                    : null);
        }
    }

    private sealed class TestUserPermissionGrantProvider(Dictionary<(string UserId, string Name), bool> values) : IPermissionGrantProvider
    {
        public Dictionary<(string UserId, string Name), bool> Values { get; } = values;

        public string Name => "TestUser";

        public int Order => 310;

        public ValueTask<PermissionGrant?> GetOrNullAsync(PermissionDefinition definition, PermissionContext context, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(context.UserId))
            {
                return ValueTask.FromResult<PermissionGrant?>(null);
            }

            return ValueTask.FromResult(
                Values.TryGetValue((context.UserId, definition.Name), out var isGranted)
                    ? new PermissionGrant(isGranted, Name)
                    : null);
        }
    }

    private sealed class AuthenticatedUserPermissionGrantProvider : IPermissionGrantProvider
    {
        public string Name => "AuthenticatedUser";

        public int Order => 320;

        public ValueTask<PermissionGrant?> GetOrNullAsync(PermissionDefinition definition, PermissionContext context, CancellationToken cancellationToken = default)
        {
            if (!context.IsAuthenticated || string.IsNullOrWhiteSpace(context.UserId))
            {
                return ValueTask.FromResult<PermissionGrant?>(null);
            }

            return ValueTask.FromResult<PermissionGrant?>(new PermissionGrant(true, Name));
        }
    }
}
