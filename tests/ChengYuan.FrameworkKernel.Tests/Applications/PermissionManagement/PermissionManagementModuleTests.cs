using ChengYuan.Authorization;
using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;
using ChengYuan.PermissionManagement;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class PermissionManagementModuleTests
{
    [Fact]
    public void PermissionManagementModule_ShouldLoadIntoTheServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddModule<PermissionManagementTestModule>();
        services.AddInMemoryPermissionManagement();

        using var serviceProvider = services.BuildServiceProvider();

        var moduleCatalog = serviceProvider.GetRequiredService<ModuleCatalog>();
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(PermissionManagementModule));
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(AuthorizationModule));

        serviceProvider.GetRequiredService<IPermissionGrantStore>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<IPermissionGrantManager>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<IPermissionChecker>().ShouldNotBeNull();
    }

    [Fact]
    public async Task PermissionManagement_ShouldApplyStoreBackedGlobalTenantAndUserGrants()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var tenantId = Guid.NewGuid();
        const string userId = "alice";

        var services = new ServiceCollection();
        services.AddModule<PermissionManagementTestModule>();
        services.AddInMemoryPermissionManagement();

        using var serviceProvider = services.BuildServiceProvider();
        var definitionManager = serviceProvider.GetRequiredService<IPermissionDefinitionManager>();
        var permissionChecker = serviceProvider.GetRequiredService<IPermissionChecker>();
        var permissionGrantManager = serviceProvider.GetRequiredService<IPermissionGrantManager>();
        var currentTenant = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        var currentUser = serviceProvider.GetRequiredService<ICurrentUserAccessor>();

        definitionManager.AddOrUpdate("workspace.members.delete").WithDefaultGrant(false);

        await permissionGrantManager.SetAsync(new PermissionGrantRecord("workspace.members.delete", PermissionScope.Global, true), cancellationToken);
        (await permissionChecker.IsGrantedAsync("workspace.members.delete", cancellationToken)).ShouldBeTrue();

        await permissionGrantManager.SetAsync(new PermissionGrantRecord("workspace.members.delete", PermissionScope.Tenant, false, tenantId), cancellationToken);

        using (currentTenant.Change(tenantId, "tenant-a"))
        {
            (await permissionChecker.IsGrantedAsync("workspace.members.delete", cancellationToken)).ShouldBeFalse();

            await permissionGrantManager.SetAsync(new PermissionGrantRecord("workspace.members.delete", PermissionScope.User, true, userId: userId), cancellationToken);

            using (currentUser.Change(new CurrentUserInfo(userId, "Alice", true)))
            {
                (await permissionChecker.IsGrantedAsync("workspace.members.delete", cancellationToken)).ShouldBeTrue();
            }
        }
    }

    [Fact]
    public async Task PermissionManagement_ShouldRemoveGrantsAndFallbackToDefinitionDefault()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var services = new ServiceCollection();
        services.AddModule<PermissionManagementTestModule>();
        services.AddInMemoryPermissionManagement();

        using var serviceProvider = services.BuildServiceProvider();
        var definitionManager = serviceProvider.GetRequiredService<IPermissionDefinitionManager>();
        var permissionChecker = serviceProvider.GetRequiredService<IPermissionChecker>();
        var permissionGrantManager = serviceProvider.GetRequiredService<IPermissionGrantManager>();

        definitionManager.AddOrUpdate("workspace.analytics.view").WithDefaultGrant(false);

        await permissionGrantManager.SetAsync(new PermissionGrantRecord("workspace.analytics.view", PermissionScope.Global, true), cancellationToken);
        (await permissionChecker.IsGrantedAsync("workspace.analytics.view", cancellationToken)).ShouldBeTrue();

        await permissionGrantManager.RemoveAsync("workspace.analytics.view", PermissionScope.Global, cancellationToken: cancellationToken);
        (await permissionChecker.IsGrantedAsync("workspace.analytics.view", cancellationToken)).ShouldBeFalse();
    }

    [Fact]
    public async Task PermissionGrantStore_ShouldReturnStoredRecords()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var tenantId = Guid.NewGuid();

        var services = new ServiceCollection();
        services.AddModule<PermissionManagementTestModule>();
        services.AddInMemoryPermissionManagement();

        using var serviceProvider = services.BuildServiceProvider();
        var permissionGrantStore = serviceProvider.GetRequiredService<IPermissionGrantStore>();

        await permissionGrantStore.SetAsync(new PermissionGrantRecord("workspace.members.list", PermissionScope.Global, true), cancellationToken);
        await permissionGrantStore.SetAsync(new PermissionGrantRecord("workspace.members.edit", PermissionScope.Tenant, false, tenantId), cancellationToken);
        await permissionGrantStore.SetAsync(new PermissionGrantRecord("workspace.members.invite", PermissionScope.User, true, userId: "alice"), cancellationToken);

        var records = await permissionGrantStore.GetListAsync(cancellationToken);
        records.Count.ShouldBe(3);
        records.Select(record => record.Name).ShouldContain("workspace.members.list");
        records.Select(record => record.Name).ShouldContain("workspace.members.edit");
        records.Select(record => record.Name).ShouldContain("workspace.members.invite");
    }
}
