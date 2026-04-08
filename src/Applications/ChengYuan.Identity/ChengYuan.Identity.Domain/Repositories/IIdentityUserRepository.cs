using ChengYuan.Core.Data;

namespace ChengYuan.Identity;

public interface IIdentityUserRepository : IRepository<IdentityUser, Guid>
{
    ValueTask<IdentityUser?> FindByNormalizedUserNameAsync(string normalizedUserName, CancellationToken cancellationToken = default);

    ValueTask<IdentityUser?> FindByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<IdentityUser>> GetListAsync(CancellationToken cancellationToken = default);
}
