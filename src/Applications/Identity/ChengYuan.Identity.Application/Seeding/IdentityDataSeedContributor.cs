using ChengYuan.Core.Data;
using ChengYuan.Core.DependencyInjection;

namespace ChengYuan.Identity;

public sealed class IdentityDataSeedContributor(
    IUserManager userManager,
    IUserReader userReader,
    IRoleManager roleManager,
    IRoleReader roleReader) : IDataSeedContributor, ITransientService
{
    private const string AdminUserName = "admin";
    private const string AdminEmail = "admin@chengyuan.dev";
    private const string AdminPassword = "Admin@123456";
    private const string AdminRoleName = "admin";

    public async ValueTask SeedAsync(DataSeedContext context, CancellationToken cancellationToken = default)
    {
        var existingRole = await roleReader.FindByNameAsync(AdminRoleName, cancellationToken);
        if (existingRole is null)
        {
            existingRole = await roleManager.CreateAsync(AdminRoleName, cancellationToken);
        }

        var existingUser = await userReader.FindByUserNameAsync(AdminUserName, cancellationToken);
        if (existingUser is null)
        {
            var user = await userManager.CreateAsync(AdminUserName, AdminEmail, AdminPassword, cancellationToken);
            await userManager.AssignRoleAsync(user.Id, existingRole.Id, cancellationToken);
        }
    }
}
