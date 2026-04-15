using ChengYuan.Core.Data;
using ChengYuan.Core.DependencyInjection;
using ChengYuan.Core.Exceptions;

namespace ChengYuan.Identity;

[ExposeServices(typeof(IUserManager), typeof(IUserReader))]
public sealed class UserManager(
    IIdentityUserRepository userRepository,
    IIdentityRoleRepository roleRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : IUserManager, IUserReader, IScopedService
{
    public async ValueTask<UserRecord> CreateAsync(string userName, string email, string password, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        await EnsureUniqueAsync(userName, email, userIdToIgnore: null, cancellationToken);

        var user = new IdentityUser(Guid.NewGuid(), userName, email);
        user.SetPasswordHash(passwordHasher.HashPassword(password));
        await userRepository.InsertAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToRecord(user);
    }

    public async ValueTask<UserRecord?> VerifyPasswordAsync(string userName, string password, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var user = await userRepository.FindByNormalizedUserNameAsync(
            IdentityUser.NormalizeUserName(userName), cancellationToken);

        if (user is null || user.IsDeleted || !user.IsActive)
        {
            return null;
        }

        if (user.PasswordHash is null || !passwordHasher.VerifyPassword(password, user.PasswordHash))
        {
            return null;
        }

        return MapToRecord(user);
    }

    public async ValueTask ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        EnsureId(userId, nameof(userId), "User");
        ArgumentException.ThrowIfNullOrWhiteSpace(currentPassword);
        ArgumentException.ThrowIfNullOrWhiteSpace(newPassword);

        var user = await userRepository.GetDetailsAsync(userId, cancellationToken);

        if (user.PasswordHash is null || !passwordHasher.VerifyPassword(currentPassword, user.PasswordHash))
        {
            throw new BusinessException(
                "The current password is incorrect.",
                new ErrorCode("Identity.InvalidPassword"));
        }

        user.SetPasswordHash(passwordHasher.HashPassword(newPassword));
        await unitOfWork.SaveChangesAsync(cancellationToken);
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
            throw new BusinessException(
                $"Role '{role.Name}' is inactive and cannot be assigned.",
                new ErrorCode("Identity.InactiveRole"));
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
            throw new BusinessException(
                $"A user named '{userName}' already exists.",
                new ErrorCode("Identity.DuplicateUserName"));
        }

        var existingUserByEmail = await userRepository.FindByNormalizedEmailAsync(normalizedEmail, cancellationToken);
        if (existingUserByEmail is not null && existingUserByEmail.Id != userIdToIgnore)
        {
            throw new BusinessException(
                $"A user with email '{email}' already exists.",
                new ErrorCode("Identity.DuplicateEmail"));
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
