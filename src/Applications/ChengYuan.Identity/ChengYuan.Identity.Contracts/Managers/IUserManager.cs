namespace ChengYuan.Identity;

public interface IUserManager
{
    ValueTask<UserRecord> CreateAsync(string userName, string email, CancellationToken cancellationToken = default);

    ValueTask<UserRecord> UpdateAsync(Guid userId, string userName, string email, bool isActive, CancellationToken cancellationToken = default);

    ValueTask RemoveAsync(Guid userId, CancellationToken cancellationToken = default);
}
