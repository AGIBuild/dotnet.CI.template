namespace ChengYuan.Identity;

public interface IUserReader
{
    ValueTask<UserRecord?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    ValueTask<UserRecord?> FindByUserNameAsync(string userName, CancellationToken cancellationToken = default);

    ValueTask<UserRecord?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<UserRecord>> GetListAsync(CancellationToken cancellationToken = default);
}
