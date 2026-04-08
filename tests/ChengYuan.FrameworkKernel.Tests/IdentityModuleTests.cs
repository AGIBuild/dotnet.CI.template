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

        serviceProvider.GetRequiredService<IUserManager>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<IUserReader>().ShouldNotBeNull();
        serviceProvider.GetRequiredService<IIdentityUserRepository>().ShouldNotBeNull();
    }

    [Fact]
    public async Task IdentityModule_ShouldCreateUpdateAndRemoveUsers()
    {
        var services = new ServiceCollection();
        services.AddModule<IdentityTestModule>();

        await using var serviceProvider = services.BuildServiceProvider();
        var userManager = serviceProvider.GetRequiredService<IUserManager>();
        var userReader = serviceProvider.GetRequiredService<IUserReader>();

        var createdUser = await userManager.CreateAsync(" Alice ", " alice@example.com ", TestContext.Current.CancellationToken);
        createdUser.UserName.ShouldBe("Alice");
        createdUser.Email.ShouldBe("alice@example.com");
        createdUser.IsActive.ShouldBeTrue();

        var updatedUser = await userManager.UpdateAsync(createdUser.Id, "Alice.Admin", "alice.admin@example.com", false, TestContext.Current.CancellationToken);
        updatedUser.UserName.ShouldBe("Alice.Admin");
        updatedUser.Email.ShouldBe("alice.admin@example.com");
        updatedUser.IsActive.ShouldBeFalse();

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

        await userManager.CreateAsync("Alice", "alice@example.com", TestContext.Current.CancellationToken);

        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await userManager.CreateAsync(" ALICE ", "another@example.com", TestContext.Current.CancellationToken));

        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await userManager.CreateAsync("Bob", " ALICE@EXAMPLE.COM ", TestContext.Current.CancellationToken));
    }
}
