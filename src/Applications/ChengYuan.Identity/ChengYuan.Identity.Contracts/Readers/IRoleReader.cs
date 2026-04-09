namespace ChengYuan.Identity;

public interface IRoleReader
{
    ValueTask<RoleRecord?> FindByIdAsync(Guid roleId, CancellationToken cancellationToken = default);

    ValueTask<RoleRecord?> FindByNameAsync(string name, CancellationToken cancellationToken = default);

    ValueTask<IReadOnlyList<RoleRecord>> GetListAsync(CancellationToken cancellationToken = default);
}
