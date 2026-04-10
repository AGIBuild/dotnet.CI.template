using ChengYuan.Core.Data;

namespace ChengYuan.Identity;

public interface IIdentityRoleRepository : IRepository<IdentityRole, Guid>
{
    ValueTask<IdentityRole?> FindByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<IdentityRole>> GetListAsync(CancellationToken cancellationToken = default);
}
