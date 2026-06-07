namespace ChengYuan.Identity;

public interface IUserSessionValidator
{
    ValueTask<bool> IsActiveSessionAsync(Guid userId, CancellationToken cancellationToken = default);
}
