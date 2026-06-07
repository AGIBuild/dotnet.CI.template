using ChengYuan.Core.Data;
using ChengYuan.Core.DependencyInjection;

namespace ChengYuan.Identity;

public sealed class IdentityDataSeedContributor(
    IUserManager userManager,
    IUserReader userReader,
    IRoleManager roleManager,
    IRoleReader roleReader,
    IdentityAdminSeedOptions adminSeedOptions) : IDataSeedContributor, ITransientService
{
    public async ValueTask SeedAsync(DataSeedContext context, CancellationToken cancellationToken = default)
    {
        if (!adminSeedOptions.SeedEnabled)
        {
            return;
        }

        var existingRole = await roleReader.FindByNameAsync(adminSeedOptions.RoleName, cancellationToken);
        if (existingRole is null)
        {
            existingRole = await roleManager.CreateAsync(adminSeedOptions.RoleName, cancellationToken);
        }

        var existingUser = await userReader.FindByUserNameAsync(adminSeedOptions.UserName, cancellationToken);
        if (existingUser is null)
        {
            var user = await userManager.CreateAsync(
                adminSeedOptions.UserName,
                adminSeedOptions.Email,
                adminSeedOptions.Password,
                cancellationToken);
            await userManager.AssignRoleAsync(user.Id, existingRole.Id, cancellationToken);
        }
    }
}
