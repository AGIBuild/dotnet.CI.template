using ChengYuan.Identity;

namespace ChengYuan.FrameworkKernel.Tests;

internal sealed class InMemoryIdentityRoleRepository : IIdentityRoleRepository
{
    private readonly object _sync = new();
    private readonly Dictionary<Guid, IdentityRole> _roles = [];

    public ValueTask<IdentityRole?> FindAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Role id cannot be empty.", nameof(id));
        }

        lock (_sync)
        {
            var role = _roles.GetValueOrDefault(id);
            return ValueTask.FromResult(role is null || role.IsDeleted ? null : role);
        }
    }

    public async ValueTask<IdentityRole> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await FindAsync(id, cancellationToken);
        return role ?? throw new InvalidOperationException($"Identity role '{id}' was not found.");
    }

    public ValueTask<IdentityRole> InsertAsync(IdentityRole entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        lock (_sync)
        {
            _roles.Add(entity.Id, entity);
        }

        return ValueTask.FromResult(entity);
    }

    public ValueTask DeleteAsync(IdentityRole entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        entity.MarkDeleted();
        return ValueTask.CompletedTask;
    }

    public ValueTask<IdentityRole?> FindByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedName);

        lock (_sync)
        {
            var role = _roles.Values.SingleOrDefault(candidate => !candidate.IsDeleted && candidate.NormalizedName == normalizedName);
            return ValueTask.FromResult(role);
        }
    }

    public ValueTask<List<IdentityRole>> GetPagedListAsync(int skipCount, int maxResultCount, string? sorting = null, CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            var result = _roles.Values
                .Where(role => !role.IsDeleted)
                .OrderBy(role => role.NormalizedName)
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
            return ValueTask.FromResult((long)_roles.Values.Count(role => !role.IsDeleted));
        }
    }

    public ValueTask<IReadOnlyList<IdentityRole>> GetListAsync(CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            var roles = _roles.Values
                .Where(role => !role.IsDeleted)
                .OrderBy(role => role.NormalizedName)
                .ThenBy(role => role.Id)
                .ToArray();

            return ValueTask.FromResult<IReadOnlyList<IdentityRole>>(roles);
        }
    }
}
