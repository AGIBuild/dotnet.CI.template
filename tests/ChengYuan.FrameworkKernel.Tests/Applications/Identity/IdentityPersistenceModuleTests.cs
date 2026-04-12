using ChengYuan.Core.Modularity;
using ChengYuan.EntityFrameworkCore;
using ChengYuan.Identity;
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
        scope.ServiceProvider.GetRequiredService<IIdentityRoleRepository>().ShouldBeOfType<EfIdentityRoleRepository>();
        scope.ServiceProvider.GetRequiredService<IIdentityUserRepository>().ShouldBeOfType<EfIdentityUserRepository>();
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

        var createdUser = await userManager.CreateAsync("Alice", "alice@example.com", TestContext.Current.CancellationToken);
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

        await userManager.CreateAsync("Alice", "alice@example.com", TestContext.Current.CancellationToken);

        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await userManager.CreateAsync("alice", "other@example.com", TestContext.Current.CancellationToken));

        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await userManager.CreateAsync("Bob", "ALICE@EXAMPLE.COM", TestContext.Current.CancellationToken));
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
        var user = await userManager.CreateAsync("Alice", "alice@example.com", TestContext.Current.CancellationToken);
        await userManager.AssignRoleAsync(user.Id, role.Id, TestContext.Current.CancellationToken);

        await Should.ThrowAsync<InvalidOperationException>(async () =>
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

        await userManager.CreateAsync("charlie", "charlie@example.com", TestContext.Current.CancellationToken);
        await userManager.CreateAsync("alice", "alice@example.com", TestContext.Current.CancellationToken);
        await userManager.CreateAsync("bob", "bob@example.com", TestContext.Current.CancellationToken);

        var users = await userReader.GetListAsync(TestContext.Current.CancellationToken);
        users.Select(user => user.UserName).ShouldBe(["alice", "bob", "charlie"]);
    }

    private static ServiceCollection CreateServices()
    {
        var databaseName = $"identity-{Guid.NewGuid():N}";
        var services = new ServiceCollection();
        services.AddModule<IdentityPersistenceTestModule>();
        services.UseDbContextOptions(options =>
            options.UseInMemoryDatabase(databaseName));

        return services;
    }
}
