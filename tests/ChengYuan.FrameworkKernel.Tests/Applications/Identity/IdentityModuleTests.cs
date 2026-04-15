using ChengYuan.Core.Exceptions;
using ChengYuan.Core.Modularity;
using ChengYuan.Identity;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public class IdentityModuleTests
{
    [Fact]
    public void IdentityModule_ShouldLoadIntoTheServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddModule<IdentityTestModule>();

        using var serviceProvider = services.BuildServiceProvider();

        var moduleCatalog = serviceProvider.GetRequiredService<ModuleCatalog>();
        moduleCatalog.ModuleTypes.Select(moduleType => moduleType.Name).ShouldContain(nameof(IdentityModule));

        serviceProvider.GetRequiredService<IRoleManager>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<IRoleReader>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<IIdentityRoleRepository>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<IUserManager>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<IUserReader>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<IIdentityUserRepository>().ShouldNotBeNull();
    }

    [Fact]
    public async Task IdentityModule_ShouldCreateAssignUpdateAndRemoveUsers()
    {
        var services = new ServiceCollection();
        services.AddModule<IdentityTestModule>();

        await using var serviceProvider = services.BuildServiceProvider();
        var roleManager = serviceProvider.GetRequiredService<IRoleManager>();
        var userManager = serviceProvider.GetRequiredService<IUserManager>();
        var userReader = serviceProvider.GetRequiredService<IUserReader>();

        var role = await roleManager.CreateAsync(" Administrators ", TestContext.Current.CancellationToken);

        var createdUser = await userManager.CreateAsync(" Alice ", " alice@example.com ", "Password123!", TestContext.Current.CancellationToken);
        createdUser.UserName.ShouldBe("Alice");
        createdUser.Email.ShouldBe("alice@example.com");
        createdUser.IsActive.ShouldBeTrue();

        var assignedUser = await userManager.AssignRoleAsync(createdUser.Id, role.Id, TestContext.Current.CancellationToken);
        assignedUser.RoleIds.ShouldBe([role.Id]);

        var updatedUser = await userManager.UpdateAsync(createdUser.Id, "Alice.Admin", "alice.admin@example.com", false, TestContext.Current.CancellationToken);
        updatedUser.UserName.ShouldBe("Alice.Admin");
        updatedUser.Email.ShouldBe("alice.admin@example.com");
        updatedUser.IsActive.ShouldBeFalse();
        updatedUser.RoleIds.ShouldBe([role.Id]);

        (await userReader.FindByUserNameAsync(" alice.admin ", TestContext.Current.CancellationToken))!.Id.ShouldBe(createdUser.Id);
        (await userReader.FindByEmailAsync("ALICE.ADMIN@EXAMPLE.COM", TestContext.Current.CancellationToken))!.Id.ShouldBe(createdUser.Id);

        await userManager.RemoveAsync(createdUser.Id, TestContext.Current.CancellationToken);

        (await userReader.FindByIdAsync(createdUser.Id, TestContext.Current.CancellationToken)).ShouldBeNull();
        (await userReader.GetListAsync(TestContext.Current.CancellationToken)).ShouldBeEmpty();
    }

    [Fact]
    public async Task IdentityModule_ShouldRejectDuplicateUserNamesAndEmails()
    {
        var services = new ServiceCollection();
        services.AddModule<IdentityTestModule>();

        await using var serviceProvider = services.BuildServiceProvider();
        var userManager = serviceProvider.GetRequiredService<IUserManager>();

        await userManager.CreateAsync("Alice", "alice@example.com", "Password123!", TestContext.Current.CancellationToken);

        await Should.ThrowAsync<BusinessException>(async () =>
            await userManager.CreateAsync(" ALICE ", "another@example.com", "Password123!", TestContext.Current.CancellationToken));

        await Should.ThrowAsync<BusinessException>(async () =>
            await userManager.CreateAsync("Bob", " ALICE@EXAMPLE.COM ", "Password123!", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task IdentityModule_ShouldRejectDuplicateRoleNamesAndRemoveAssignmentsWhenRolesAreDeleted()
    {
        var services = new ServiceCollection();
        services.AddModule<IdentityTestModule>();

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
}
