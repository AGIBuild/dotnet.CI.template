using ChengYuan.Core.Data;
using ChengYuan.Core.DependencyInjection;

namespace ChengYuan.Identity;

[ExposeServices(typeof(IUserManager), typeof(IUserReader))]
public sealed class UserManager(
    IIdentityUserRepository userRepository,
    IIdentityRoleRepository roleRepository,
    IUnitOfWork unitOfWork) : IUserManager, IUserReader, IScopedService
{
    public async ValueTask<UserRecord> CreateAsync(string userName, string email, CancellationToken cancellationToken = default)
    {
        await EnsureUniqueAsync(userName, email, userIdToIgnore: null, cancellationToken);

        var user = new IdentityUser(Guid.NewGuid(), userName, email);
        await userRepository.InsertAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToRecord(user);
    }

    public async ValueTask<UserRecord> UpdateAsync(Guid userId, string userName, string email, bool isActive, CancellationToken cancellationToken = default)
    {
        EnsureId(userId, nameof(userId), "User");

        await EnsureUniqueAsync(userName, email, userId, cancellationToken);

        var user = await userRepository.GetDetailsAsync(userId, cancellationToken);
        user.Update(userName, email, isActive);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToRecord(user);
    }

    public async ValueTask<UserRecord> AssignRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        EnsureId(userId, nameof(userId), "User");
        EnsureId(roleId, nameof(roleId), "Role");

        var user = await userRepository.GetDetailsAsync(userId, cancellationToken);
        var role = await roleRepository.GetAsync(roleId, cancellationToken);

        if (!role.IsActive)
        {
            throw new InvalidOperationException($"Role '{role.Name}' is inactive and cannot be assigned.");
        }

        user.AssignRole(roleId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToRecord(user);
    }

    public async ValueTask<UserRecord> UnassignRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
    {
        EnsureId(userId, nameof(userId), "User");
        EnsureId(roleId, nameof(roleId), "Role");

        var user = await userRepository.GetDetailsAsync(userId, cancellationToken);
        user.UnassignRole(roleId);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToRecord(user);
    }

    public async ValueTask RemoveAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        EnsureId(userId, nameof(userId), "User");

        var user = await userRepository.GetDetailsAsync(userId, cancellationToken);
        user.MarkDeleted();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask<UserRecord?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        EnsureId(userId, nameof(userId), "User");

        var user = await userRepository.FindDetailsAsync(userId, cancellationToken);
        return user is null ? null : MapToRecord(user);
    }

    public async ValueTask<UserRecord?> FindByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.FindByNormalizedUserNameAsync(IdentityUser.NormalizeUserName(userName), cancellationToken);
        return user is null ? null : MapToRecord(user);
    }

    public async ValueTask<UserRecord?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.FindByNormalizedEmailAsync(IdentityUser.NormalizeEmail(email), cancellationToken);
        return user is null ? null : MapToRecord(user);
    }

    public async ValueTask<IReadOnlyList<UserRecord>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var users = await userRepository.GetListAsync(cancellationToken);
        return users.Select(MapToRecord).ToArray();
    }

    private async ValueTask EnsureUniqueAsync(string userName, string email, Guid? userIdToIgnore, CancellationToken cancellationToken)
    {
        var normalizedUserName = IdentityUser.NormalizeUserName(userName);
        var normalizedEmail = IdentityUser.NormalizeEmail(email);

        var existingUserByUserName = await userRepository.FindByNormalizedUserNameAsync(normalizedUserName, cancellationToken);
        if (existingUserByUserName is not null && existingUserByUserName.Id != userIdToIgnore)
        {
            throw new InvalidOperationException($"A user named '{userName}' already exists.");
        }

        var existingUserByEmail = await userRepository.FindByNormalizedEmailAsync(normalizedEmail, cancellationToken);
        if (existingUserByEmail is not null && existingUserByEmail.Id != userIdToIgnore)
        {
            throw new InvalidOperationException($"A user with email '{email}' already exists.");
        }
    }

    private static UserRecord MapToRecord(IdentityUser user)
    {
        return new UserRecord(
            user.Id,
            user.UserName,
            user.Email,
            user.IsActive,
            user.Roles.Select(userRole => userRole.RoleId).OrderBy(roleId => roleId).ToArray());
    }

    private static void EnsureId(Guid id, string parameterName, string label)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException($"{label} id cannot be empty.", parameterName);
        }
    }
}
