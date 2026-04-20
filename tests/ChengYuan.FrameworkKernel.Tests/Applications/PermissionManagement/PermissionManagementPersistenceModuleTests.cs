using ChengYuan.Authorization;
using ChengYuan.Core.Modularity;
using ChengYuan.EntityFrameworkCore;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;
using ChengYuan.PermissionManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class PermissionManagementPersistenceModuleTests
{
    [Fact]
    public void PermissionManagementPersistenceModule_ShouldLoadIntoTheServiceCollection()
    {
        var services = CreateServices();

        using var serviceProvider = services.BuildServiceProvider();

        var moduleCatalog = serviceProvider.GetRequiredService<ModuleCatalog>();
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(PermissionManagementPersistenceModule));
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(PermissionManagementModule));

        using var scope = serviceProvider.CreateScope();
        scope.ServiceProvider.GetRequiredService<PermissionManagementDbContext>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<IPermissionGrantStore>().ShouldBeOfType<PermissionGrantStore>();
        serviceProvider.GetRequiredService<IPermissionGrantReader>().ShouldBeOfType<PermissionGrantStore>();
        serviceProvider.GetRequiredService<IPermissionGrantManager>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<IPermissionChecker>().ShouldNotBeNull();
    }

    [Fact]
    public async Task PermissionManagementPersistence_ShouldApplyStoreBackedGrantsWithProhibitedOverride()
    {
        var services = CreateServices();
        services.AddSingleton<IPermissionDefinitionContributor>(new TestContributor(context =>
        {
            var group = context.AddGroup("workspace", "Workspace");
            group.AddPermission("workspace.members.delete", "Delete Members");
        }));

        await using var serviceProvider = services.BuildServiceProvider();
        var permissionChecker = serviceProvider.GetRequiredService<IPermissionChecker>();
        var permissionGrantManager = serviceProvider.GetRequiredService<IPermissionGrantManager>();
        var currentTenant = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var currentUser = serviceProvider.GetRequiredService<ICurrentUserAccessor>();
        var tenantId = Guid.NewGuid();

        // Global=Granted → true
        await permissionGrantManager.SetAsync(new PermissionGrantRecord("workspace.members.delete", PermissionScope.Global, true), TestContext.Current.CancellationToken);
        (await permissionChecker.IsGrantedAsync("workspace.members.delete", TestContext.Current.CancellationToken)).ShouldBeTrue();

        // Add Tenant=Prohibited → any Prohibited overrides → false
        await permissionGrantManager.SetAsync(new PermissionGrantRecord("workspace.members.delete", PermissionScope.Tenant, false, tenantId), TestContext.Current.CancellationToken);

        using (currentTenant.Change(tenantId, "tenant-a"))
        {
            (await permissionChecker.IsGrantedAsync("workspace.members.delete", TestContext.Current.CancellationToken)).ShouldBeFalse();

            // Add User=Granted, but Tenant still Prohibited → Prohibited wins → false
            await permissionGrantManager.SetAsync(new PermissionGrantRecord("workspace.members.delete", PermissionScope.User, true, userId: "alice"), TestContext.Current.CancellationToken);

            using (currentUser.Change(new CurrentUserInfo("alice", "Alice", true)))
            {
                (await permissionChecker.IsGrantedAsync("workspace.members.delete", TestContext.Current.CancellationToken)).ShouldBeFalse();
            }
        }
    }

    [Fact]
    public async Task PermissionManagementPersistence_ShouldRemoveGrantsAndFallbackToDefinitionDefault()
    {
        var services = CreateServices();
        services.AddSingleton<IPermissionDefinitionContributor>(new TestContributor(context =>
        {
            var group = context.AddGroup("workspace", "Workspace");
            group.AddPermission("workspace.analytics.view", "View Analytics");
        }));

        await using var serviceProvider = services.BuildServiceProvider();
        var permissionChecker = serviceProvider.GetRequiredService<IPermissionChecker>();
        var permissionGrantManager = serviceProvider.GetRequiredService<IPermissionGrantManager>();

        await permissionGrantManager.SetAsync(new PermissionGrantRecord("workspace.analytics.view", PermissionScope.Global, true), TestContext.Current.CancellationToken);
        (await permissionChecker.IsGrantedAsync("workspace.analytics.view", TestContext.Current.CancellationToken)).ShouldBeTrue();

        await permissionGrantManager.RemoveAsync("workspace.analytics.view", PermissionScope.Global, cancellationToken: TestContext.Current.CancellationToken);
        (await permissionChecker.IsGrantedAsync("workspace.analytics.view", TestContext.Current.CancellationToken)).ShouldBeFalse();
    }

    [Fact]
    public async Task PermissionManagementPersistence_ShouldReturnStoredRecords()
    {
        var services = CreateServices();

        await using var serviceProvider = services.BuildServiceProvider();
        var permissionGrantStore = serviceProvider.GetRequiredService<IPermissionGrantStore>();

        await permissionGrantStore.SetAsync(new PermissionGrantRecord("workspace.members.list", PermissionScope.Global, true), TestContext.Current.CancellationToken);
        await permissionGrantStore.SetAsync(new PermissionGrantRecord("workspace.members.edit", PermissionScope.Tenant, false, Guid.NewGuid()), TestContext.Current.CancellationToken);
        await permissionGrantStore.SetAsync(new PermissionGrantRecord("workspace.members.invite", PermissionScope.User, true, userId: "alice"), TestContext.Current.CancellationToken);

        var records = await permissionGrantStore.GetListAsync(TestContext.Current.CancellationToken);
        records.Count.ShouldBe(3);
        records.Select(record => record.Name).ShouldContain("workspace.members.list");
        records.Select(record => record.Name).ShouldContain("workspace.members.edit");
        records.Select(record => record.Name).ShouldContain("workspace.members.invite");
    }

    private static ServiceCollection CreateServices()
    {
        var databaseName = $"permission-management-{Guid.NewGuid():N}";
        var services = new ServiceCollection();
        services.AddModule<PermissionManagementPersistenceTestModule>();
        services.UseDbContextOptions(options =>
            options.UseInMemoryDatabase(databaseName));

        return services;
    }

    private sealed class TestContributor(Action<IPermissionDefinitionContext> configure) : IPermissionDefinitionContributor
    {
        public void Define(IPermissionDefinitionContext context) => configure(context);
    }
}
