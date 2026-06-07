using ChengYuan.Core.Exceptions;
using ChengYuan.Core.Modularity;
using ChengYuan.EntityFrameworkCore;
using ChengYuan.Identity;
using ChengYuan.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class IdentityPersistenceModuleTests
{
    [Fact]
    public void IdentityPersistenceModule_ShouldLoadIntoTheServiceCollection()
    {
        var services = CreateServices();

        using var serviceProvider = services.BuildServiceProvider();

        var moduleCatalog = serviceProvider.GetRequiredService<ModuleCatalog>();
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(IdentityPersistenceModule));
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(IdentityModule));

        using var scope = serviceProvider.CreateScope();
        scope.ServiceProvider.GetRequiredService<IdentityDbContext>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<IIdentityRoleRepository>().ShouldBeOfType<IdentityRoleRepository>();
        scope.ServiceProvider.GetRequiredService<IIdentityUserRepository>().ShouldBeOfType<IdentityUserRepository>();
        scope.ServiceProvider.GetRequiredService<IRoleManager>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<IRoleReader>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<IUserManager>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<IUserReader>().ShouldNotBeNull();
    }

    [Fact]
    public async Task IdentityPersistence_ShouldCreateAssignUpdateAndRemoveUsers()
    {
        var services = CreateServices();

        await using var serviceProvider = services.BuildServiceProvider();
        var roleManager = serviceProvider.GetRequiredService<IRoleManager>();
        var userManager = serviceProvider.GetRequiredService<IUserManager>();
        var userReader = serviceProvider.GetRequiredService<IUserReader>();

        var role = await roleManager.CreateAsync("Administrators", TestContext.Current.CancellationToken);

        var createdUser = await userManager.CreateAsync("Alice", "alice@example.com", "Password123!", TestContext.Current.CancellationToken);
        createdUser.IsActive.ShouldBeTrue();

        var assignedUser = await userManager.AssignRoleAsync(createdUser.Id, role.Id, TestContext.Current.CancellationToken);
        assignedUser.RoleIds.ShouldBe([role.Id]);

        var updatedUser = await userManager.UpdateAsync(createdUser.Id, "Alice.Admin", "alice.admin@example.com", false, TestContext.Current.CancellationToken);
        updatedUser.UserName.ShouldBe("Alice.Admin");
        updatedUser.Email.ShouldBe("alice.admin@example.com");
        updatedUser.IsActive.ShouldBeFalse();
        updatedUser.RoleIds.ShouldBe([role.Id]);

        (await userReader.FindByUserNameAsync("ALICE.ADMIN", TestContext.Current.CancellationToken))!.Id.ShouldBe(createdUser.Id);
        (await userReader.FindByEmailAsync("ALICE.ADMIN@EXAMPLE.COM", TestContext.Current.CancellationToken))!.Id.ShouldBe(createdUser.Id);

        await userManager.RemoveAsync(createdUser.Id, TestContext.Current.CancellationToken);

        (await userReader.FindByIdAsync(createdUser.Id, TestContext.Current.CancellationToken)).ShouldBeNull();
    }

    [Fact]
    public async Task IdentityPersistence_ShouldRejectDuplicateUserNamesAndEmails()
    {
        var services = CreateServices();

        await using var serviceProvider = services.BuildServiceProvider();
        var userManager = serviceProvider.GetRequiredService<IUserManager>();

        await userManager.CreateAsync("Alice", "alice@example.com", "Password123!", TestContext.Current.CancellationToken);

        await Should.ThrowAsync<BusinessException>(async () =>
            await userManager.CreateAsync("alice", "other@example.com", "Password123!", TestContext.Current.CancellationToken));

        await Should.ThrowAsync<BusinessException>(async () =>
            await userManager.CreateAsync("Bob", "ALICE@EXAMPLE.COM", "Password123!", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task IdentityPersistence_ShouldRejectDuplicateRoleNamesAndRemoveAssignmentsWhenRolesAreDeleted()
    {
        var services = CreateServices();

        await using var serviceProvider = services.BuildServiceProvider();
        var roleManager = serviceProvider.GetRequiredService<IRoleManager>();
        var roleReader = serviceProvider.GetRequiredService<IRoleReader>();
        var userManager = serviceProvider.GetRequiredService<IUserManager>();
        var userReader = serviceProvider.GetRequiredService<IUserReader>();

        var role = await roleManager.CreateAsync("Administrators", TestContext.Current.CancellationToken);
        var user = await userManager.CreateAsync("Alice", "alice@example.com", "Password123!", TestContext.Current.CancellationToken);
        await userManager.AssignRoleAsync(user.Id, role.Id, TestContext.Current.CancellationToken);

        await Should.ThrowAsync<BusinessException>(async () =>
            await roleManager.CreateAsync(" administrators ", TestContext.Current.CancellationToken));

        await roleManager.RemoveAsync(role.Id, TestContext.Current.CancellationToken);

        (await roleReader.FindByIdAsync(role.Id, TestContext.Current.CancellationToken)).ShouldBeNull();
        (await userReader.FindByIdAsync(user.Id, TestContext.Current.CancellationToken))!.RoleIds.ShouldBeEmpty();
    }

    [Fact]
    public async Task IdentityPersistence_ShouldReturnUsersOrderedByNormalizedUserName()
    {
        var services = CreateServices();

        await using var serviceProvider = services.BuildServiceProvider();
        var userManager = serviceProvider.GetRequiredService<IUserManager>();
        var userReader = serviceProvider.GetRequiredService<IUserReader>();

        await userManager.CreateAsync("charlie", "charlie@example.com", "Password123!", TestContext.Current.CancellationToken);
        await userManager.CreateAsync("alice", "alice@example.com", "Password123!", TestContext.Current.CancellationToken);
        await userManager.CreateAsync("bob", "bob@example.com", "Password123!", TestContext.Current.CancellationToken);

        var users = await userReader.GetListAsync(TestContext.Current.CancellationToken);
        users.Select(user => user.UserName).ShouldBe(["alice", "bob", "charlie"]);
    }

    [Fact]
    public async Task IdentityPersistence_ShouldManageUserTenantMemberships()
    {
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var services = CreateServices(tenantId, otherTenantId);

        await using var serviceProvider = services.BuildServiceProvider();
        var userManager = serviceProvider.GetRequiredService<IUserManager>();
        var membershipManager = serviceProvider.GetRequiredService<IUserTenantMembershipManager>();
        var membershipReader = serviceProvider.GetRequiredService<IUserTenantMembershipReader>();

        var user = await userManager.CreateAsync("Alice", "alice@example.com", "Password123!", TestContext.Current.CancellationToken);

        var membership = await membershipManager.AssignAsync(user.Id, tenantId, TestContext.Current.CancellationToken);
        membership.UserId.ShouldBe(user.Id);
        membership.TenantId.ShouldBe(tenantId);
        membership.IsActive.ShouldBeTrue();

        (await membershipReader.IsActiveMemberAsync(user.Id, tenantId, TestContext.Current.CancellationToken)).ShouldBeTrue();
        (await membershipReader.GetListByUserIdAsync(user.Id, TestContext.Current.CancellationToken))
            .Select(record => record.TenantId)
            .ShouldBe([tenantId]);

        var currentTenantAccessor = serviceProvider.GetRequiredService<ICurrentTenantAccessor>();
        using (currentTenantAccessor.Change(otherTenantId, "other"))
        {
            (await membershipReader.IsActiveMemberAsync(user.Id, tenantId, TestContext.Current.CancellationToken)).ShouldBeTrue();
        }

        await membershipManager.RevokeAsync(user.Id, tenantId, TestContext.Current.CancellationToken);
        (await membershipReader.IsActiveMemberAsync(user.Id, tenantId, TestContext.Current.CancellationToken)).ShouldBeFalse();
    }

    private static ServiceCollection CreateServices(params Guid[] tenantIds)
    {
        var databaseName = $"identity-{Guid.NewGuid():N}";
        var services = new ServiceCollection();
        services.AddModule<IdentityPersistenceTestModule>();
        services.UseDbContextOptions(options =>
            options.UseInMemoryDatabase(databaseName));
        if (tenantIds.Length > 0)
        {
            services.AddSingleton<ITenantResolutionStore>(
                new InMemoryTenantResolutionStore(tenantIds.Select(tenantId =>
                    new TenantResolutionRecord(tenantId, $"Tenant-{tenantId:N}", true)).ToArray()));
        }

        return services;
    }
}
