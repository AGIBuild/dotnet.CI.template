using ChengYuan.Core.Data;
using ChengYuan.Core.DependencyInjection;
using ChengYuan.Core.Exceptions;

namespace ChengYuan.Identity;

[ExposeServices(typeof(IRoleManager), typeof(IRoleReader))]
public sealed class RoleManager(
    IIdentityRoleRepository roleRepository,
    IIdentityUserRepository userRepository,
    IUnitOfWork unitOfWork) : IRoleManager, IRoleReader, IScopedService
{
    public async ValueTask<RoleRecord> CreateAsync(string name, CancellationToken cancellationToken = default)
    {
        await EnsureUniqueAsync(name, roleIdToIgnore: null, cancellationToken);

        var role = new IdentityRole(Guid.NewGuid(), name);
        await roleRepository.InsertAsync(role, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToRecord(role);
    }

    public async ValueTask<RoleRecord> UpdateAsync(Guid roleId, string name, bool isActive, CancellationToken cancellationToken = default)
    {
        EnsureId(roleId, nameof(roleId), "Role");
        await EnsureUniqueAsync(name, roleId, cancellationToken);

        var role = await roleRepository.GetAsync(roleId, cancellationToken);
        role.Update(name, isActive);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToRecord(role);
    }

    public async ValueTask RemoveAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        EnsureId(roleId, nameof(roleId), "Role");

        var role = await roleRepository.GetAsync(roleId, cancellationToken);
        var users = await userRepository.GetListByRoleIdAsync(roleId, cancellationToken);

        foreach (var user in users)
        {
            user.UnassignRole(roleId);
        }

        role.MarkDeleted();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask<RoleRecord?> FindByIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        EnsureId(roleId, nameof(roleId), "Role");

        var role = await roleRepository.FindAsync(roleId, cancellationToken);
        return role is null ? null : MapToRecord(role);
    }

    public async ValueTask<RoleRecord?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var role = await roleRepository.FindByNormalizedNameAsync(IdentityRole.NormalizeName(name), cancellationToken);
        return role is null ? null : MapToRecord(role);
    }

    public async ValueTask<IReadOnlyList<RoleRecord>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var roles = await roleRepository.GetListAsync(cancellationToken);
        return roles.Select(MapToRecord).ToArray();
    }

    private async ValueTask EnsureUniqueAsync(string name, Guid? roleIdToIgnore, CancellationToken cancellationToken)
    {
        var existingRole = await roleRepository.FindByNormalizedNameAsync(IdentityRole.NormalizeName(name), cancellationToken);
        if (existingRole is not null && existingRole.Id != roleIdToIgnore)
        {
            throw new BusinessException(
                $"A role named '{name}' already exists.",
                new ErrorCode("Identity.DuplicateRoleName"));
        }
    }

    private static RoleRecord MapToRecord(IdentityRole role)
    {
        return new RoleRecord(role.Id, role.Name, role.IsActive);
    }

    private static void EnsureId(Guid id, string parameterName, string label)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException($"{label} id cannot be empty.", parameterName);
        }
    }
}
