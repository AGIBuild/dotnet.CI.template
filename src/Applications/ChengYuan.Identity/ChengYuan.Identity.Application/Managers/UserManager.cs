using ChengYuan.Core.Data;

namespace ChengYuan.Identity;

public sealed class UserManager(IIdentityUserRepository userRepository, IUnitOfWork unitOfWork) : IUserManager, IUserReader
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
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        await EnsureUniqueAsync(userName, email, userId, cancellationToken);

        var user = await userRepository.GetAsync(userId, cancellationToken);
        user.Update(userName, email, isActive);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToRecord(user);
    }

    public async ValueTask RemoveAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        var user = await userRepository.GetAsync(userId, cancellationToken);
        user.MarkDeleted();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask<UserRecord?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(userId));
        }

        var user = await userRepository.FindAsync(userId, cancellationToken);
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
        return new UserRecord(user.Id, user.UserName, user.Email, user.IsActive);
    }
}
