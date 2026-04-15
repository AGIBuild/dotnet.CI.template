using ChengYuan.Identity;

namespace ChengYuan.FrameworkKernel.Tests;

internal sealed class InMemoryIdentityUserRepository : IIdentityUserRepository
{
    private readonly object _sync = new();
    private readonly Dictionary<Guid, IdentityUser> _users = [];

    public ValueTask<IdentityUser?> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(id));
        }

        lock (_sync)
        {
            var user = _users.GetValueOrDefault(id);
            return ValueTask.FromResult(user is null || user.IsDeleted ? null : user);
        }
    }

    public ValueTask<IdentityUser?> FindDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return FindAsync(id, cancellationToken);
    }

    public async ValueTask<IdentityUser> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await FindAsync(id, cancellationToken);
        return user ?? throw new InvalidOperationException($"Identity user '{id}' was not found.");
    }

    public async ValueTask<IdentityUser> GetDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await FindDetailsAsync(id, cancellationToken);
        return user ?? throw new InvalidOperationException($"Identity user '{id}' was not found.");
    }

    public ValueTask<IdentityUser> InsertAsync(IdentityUser entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        lock (_sync)
        {
            _users.Add(entity.Id, entity);
        }

        return ValueTask.FromResult(entity);
    }

    public ValueTask DeleteAsync(IdentityUser entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        entity.MarkDeleted();
        return ValueTask.CompletedTask;
    }

    public ValueTask<IdentityUser?> FindByNormalizedUserNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedUserName);

        lock (_sync)
        {
            var user = _users.Values.SingleOrDefault(candidate => !candidate.IsDeleted && candidate.NormalizedUserName == normalizedUserName);
            return ValueTask.FromResult(user);
        }
    }

    public ValueTask<IdentityUser?> FindByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedEmail);

        lock (_sync)
        {
            var user = _users.Values.SingleOrDefault(candidate => !candidate.IsDeleted && candidate.NormalizedEmail == normalizedEmail);
            return ValueTask.FromResult(user);
        }
    }

    public ValueTask<IReadOnlyList<IdentityUser>> GetListByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        if (roleId == Guid.Empty)
        {
            throw new ArgumentException("Role id cannot be empty.", nameof(roleId));
        }

        lock (_sync)
        {
            var users = _users.Values
                .Where(user => !user.IsDeleted && user.Roles.Any(role => role.RoleId == roleId))
                .OrderBy(user => user.NormalizedUserName)
                .ThenBy(user => user.Id)
                .ToArray();

            return ValueTask.FromResult<IReadOnlyList<IdentityUser>>(users);
        }
    }

    public ValueTask<List<IdentityUser>> GetPagedListAsync(int skipCount, int maxResultCount, string? sorting = null, CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            var result = _users.Values
                .Where(user => !user.IsDeleted)
                .OrderBy(user => user.NormalizedUserName)
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToList();

            return ValueTask.FromResult(result);
        }
    }

    public ValueTask<long> GetCountAsync(CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            return ValueTask.FromResult((long)_users.Values.Count(user => !user.IsDeleted));
        }
    }

    public ValueTask<IReadOnlyList<IdentityUser>> GetListAsync(CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            var users = _users.Values
                .Where(user => !user.IsDeleted)
                .OrderBy(user => user.NormalizedUserName)
                .ThenBy(user => user.Id)
                .ToArray();

            return ValueTask.FromResult<IReadOnlyList<IdentityUser>>(users);
        }
    }
}
