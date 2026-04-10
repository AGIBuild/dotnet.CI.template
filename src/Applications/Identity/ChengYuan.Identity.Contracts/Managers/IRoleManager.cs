namespace ChengYuan.Identity;

public interface IRoleManager
{
    ValueTask<RoleRecord> CreateAsync(string name, CancellationToken cancellationToken = default);

    ValueTask<RoleRecord> UpdateAsync(Guid roleId, string name, bool isActive, CancellationToken cancellationToken = default);

    ValueTask RemoveAsync(Guid roleId, CancellationToken cancellationToken = default);
}
