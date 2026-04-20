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
        services.AddSingleton<IPermissionDefinitionContributor>(new TestContributor(context =>
        {
            var group = context.AddGroup("workspace.members", "Workspace Members");
            var permission = group.AddPermission("workspace.members.delete", "Delete workspace members");
            permission.DefaultGranted = false;
            permission.Description = "Allows deleting members from the current workspace.";
        }));

        using var serviceProvider = services.BuildServiceProvider();
        var manager = serviceProvider.GetRequiredService<IPermissionDefinitionManager>();

        manager.IsDefined("workspace.members.delete").ShouldBeTrue();
        var definition = manager.GetPermission("workspace.members.delete");
        definition.DefaultGranted.ShouldBeFalse();
        definition.DisplayName.ShouldBe("Delete workspace members");
        definition.Description.ShouldBe("Allows deleting members from the current workspace.");
        definition.Group.ShouldNotBeNull();
        definition.Group!.Name.ShouldBe("workspace.members");
        manager.GetGroups().Count.ShouldBe(1);
        manager.GetAll().Count.ShouldBe(1);
    }

    [Fact]
    public async Task PermissionChecker_ShouldFallbackToDefinitionDefault()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddModule<AuthorizationTestModule>();
        services.AddSingleton<IPermissionDefinitionContributor>(new TestContributor(context =>
        {
            var group = context.AddGroup("workspace", "Workspace");
            var permission = group.AddPermission("workspace.analytics.view", "View Analytics");
            permission.DefaultGranted = true;
        }));

        using var serviceProvider = services.BuildServiceProvider();
        var checker = serviceProvider.GetRequiredService<IPermissionChecker>();

        (await checker.IsGrantedAsync("workspace.analytics.view", cancellationToken)).ShouldBeTrue();
    }

    [Fact]
    public async Task PermissionChecker_ProhibitedShouldOverrideGranted()
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
        services.AddSingleton<IPermissionDefinitionContributor>(new TestContributor(context =>
        {
            var group = context.AddGroup("workspace", "Workspace");
            var permission = group.AddPermission("workspace.members.delete", "Delete Members");
            permission.DefaultGranted = false;
        }));

        using var serviceProvider = services.BuildServiceProvider();
        var checker = serviceProvider.GetRequiredService<IPermissionChecker>();
        var currentTenant = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var currentUser = serviceProvider.GetRequiredService<ICurrentUserAccessor>();

        // Global=Granted, no tenant/user context → Granted
        (await checker.IsGrantedAsync("workspace.members.delete", cancellationToken)).ShouldBeTrue();

        var tenantId = Guid.NewGuid();
        const string userId = "alice";

        // Add Tenant=Prohibited → any Prohibited overrides → false
        var tenantProvider = serviceProvider.GetServices<IPermissionGrantProvider>().OfType<TestTenantPermissionGrantProvider>().Single();
        tenantProvider.Values[(tenantId, "workspace.members.delete")] = false;

        using (currentTenant.Change(tenantId, "tenant-a"))
        {
            (await checker.IsGrantedAsync("workspace.members.delete", cancellationToken)).ShouldBeFalse();

            // Add User=Granted, but Tenant still Prohibited → still false
            var userProvider = serviceProvider.GetServices<IPermissionGrantProvider>().OfType<TestUserPermissionGrantProvider>().Single();
            userProvider.Values[(userId, "workspace.members.delete")] = true;

            using (currentUser.Change(new CurrentUserInfo(userId, "Alice", true)))
            {
                (await checker.IsGrantedAsync("workspace.members.delete", cancellationToken)).ShouldBeFalse();
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
        services.AddSingleton<IPermissionDefinitionContributor>(new TestContributor(context =>
        {
            var group = context.AddGroup("workspace", "Workspace");
            var permission = group.AddPermission("workspace.analytics.view", "View Analytics");
            permission.DefaultGranted = false;
        }));

        using var serviceProvider = services.BuildServiceProvider();
        var checker = serviceProvider.GetRequiredService<IPermissionChecker>();
        var currentUser = serviceProvider.GetRequiredService<ICurrentUserAccessor>();

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
        services.AddSingleton<IPermissionDefinitionContributor>(new TestContributor(context =>
        {
            var group = context.AddGroup("workspace", "Workspace");
            var permission = group.AddPermission("workspace.members.delete", "Delete Members");
            permission.DefaultGranted = true;
        }));
        services.AddInMemoryPermissions(builder => builder
            .SetGlobal("workspace.members.delete", false)
            .SetTenant("workspace.members.delete", tenantId, true)
            .SetUser("workspace.members.delete", userId, false));

        using var serviceProvider = services.BuildServiceProvider();
        var checker = serviceProvider.GetRequiredService<IPermissionChecker>();
        var currentTenant = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var currentUser = serviceProvider.GetRequiredService<ICurrentUserAccessor>();

        // Global=Prohibited → false (Prohibited wins over DefaultGranted)
        (await checker.IsGrantedAsync("workspace.members.delete", cancellationToken)).ShouldBeFalse();

        // With tenant context: Global=Prohibited + Tenant=Granted → Prohibited wins → false
        using (currentTenant.Change(tenantId, "tenant-a"))
        {
            (await checker.IsGrantedAsync("workspace.members.delete", cancellationToken)).ShouldBeFalse();

            // With tenant+user context: Global=Prohibited + Tenant=Granted + User=Prohibited → Prohibited wins → false
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
    private sealed class AuthorizationTestModule : FrameworkCoreModule
    {
    }

    private sealed class TestContributor(Action<IPermissionDefinitionContext> configure) : IPermissionDefinitionContributor
    {
        public void Define(IPermissionDefinitionContext context) => configure(context);
    }

    private sealed class TestGlobalPermissionGrantProvider(Dictionary<string, bool> values) : IPermissionGrantProvider
    {
        public Dictionary<string, bool> Values { get; } = values;

        public string Name => "TestGlobal";

        public int Order => 110;

        public ValueTask<PermissionGrantResult> CheckAsync(PermissionDefinition definition, PermissionContext context, CancellationToken cancellationToken = default)
        {
            if (Values.TryGetValue(definition.Name, out var isGranted))
            {
                return ValueTask.FromResult(isGranted ? PermissionGrantResult.Granted : PermissionGrantResult.Prohibited);
            }

            return ValueTask.FromResult(PermissionGrantResult.Undefined);
        }
    }

    private sealed class TestTenantPermissionGrantProvider(Dictionary<(Guid TenantId, string Name), bool> values) : IPermissionGrantProvider
    {
        public Dictionary<(Guid TenantId, string Name), bool> Values { get; } = values;

        public string Name => "TestTenant";

        public int Order => 210;

        public ValueTask<PermissionGrantResult> CheckAsync(PermissionDefinition definition, PermissionContext context, CancellationToken cancellationToken = default)
        {
            if (context.TenantId is not Guid tenantId)
            {
                return ValueTask.FromResult(PermissionGrantResult.Undefined);
            }

            if (Values.TryGetValue((tenantId, definition.Name), out var isGranted))
            {
                return ValueTask.FromResult(isGranted ? PermissionGrantResult.Granted : PermissionGrantResult.Prohibited);
            }

            return ValueTask.FromResult(PermissionGrantResult.Undefined);
        }
    }

    private sealed class TestUserPermissionGrantProvider(Dictionary<(string UserId, string Name), bool> values) : IPermissionGrantProvider
    {
        public Dictionary<(string UserId, string Name), bool> Values { get; } = values;

        public string Name => "TestUser";

        public int Order => 310;

        public ValueTask<PermissionGrantResult> CheckAsync(PermissionDefinition definition, PermissionContext context, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(context.UserId))
            {
                return ValueTask.FromResult(PermissionGrantResult.Undefined);
            }

            if (Values.TryGetValue((context.UserId, definition.Name), out var isGranted))
            {
                return ValueTask.FromResult(isGranted ? PermissionGrantResult.Granted : PermissionGrantResult.Prohibited);
            }

            return ValueTask.FromResult(PermissionGrantResult.Undefined);
        }
    }

    private sealed class AuthenticatedUserPermissionGrantProvider : IPermissionGrantProvider
    {
        public string Name => "AuthenticatedUser";

        public int Order => 320;

        public ValueTask<PermissionGrantResult> CheckAsync(PermissionDefinition definition, PermissionContext context, CancellationToken cancellationToken = default)
        {
            if (!context.IsAuthenticated || string.IsNullOrWhiteSpace(context.UserId))
            {
                return ValueTask.FromResult(PermissionGrantResult.Undefined);
            }

            return ValueTask.FromResult(PermissionGrantResult.Granted);
        }
    }
}
