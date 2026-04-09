using ChengYuan.Core.Data;

namespace ChengYuan.Identity;

public interface IIdentityUserRepository : IRepository<IdentityUser, Guid>
{
    ValueTask<IdentityUser?> FindDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    ValueTask<IdentityUser> GetDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    ValueTask<IdentityUser?> FindByNormalizedUserNameAsync(string normalizedUserName, CancellationToken cancellationToken = default);

    ValueTask<IdentityUser?> FindByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<IdentityUser>> GetListByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<IdentityUser>> GetListAsync(CancellationToken cancellationToken = default);
}
