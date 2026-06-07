using ChengYuan.Core.Data;
using ChengYuan.Core.DependencyInjection;
using ChengYuan.Core.Exceptions;
using ChengYuan.MultiTenancy;

namespace ChengYuan.Identity;

[ExposeServices(typeof(IUserTenantMembershipManager), typeof(IUserTenantMembershipReader))]
public sealed class UserTenantMembershipManager(
    IIdentityUserRepository userRepository,
    IIdentityUserTenantMembershipRepository membershipRepository,
    ICurrentTenant currentTenant,
    ITenantResolutionStore tenantResolutionStore,
    ITenantScopeAccessPolicy tenantScopeAccessPolicy,
    IUnitOfWork unitOfWork) : IUserTenantMembershipManager, IUserTenantMembershipReader, IScopedService
{
    public async ValueTask<UserTenantMembershipRecord> AssignAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        EnsureHostContext();
        EnsureId(userId, nameof(userId), "User");
        EnsureId(tenantId, nameof(tenantId), "Tenant");

        await userRepository.GetDetailsAsync(userId, cancellationToken);
        await EnsureTenantIsActiveAsync(tenantId, cancellationToken);

        var membership = await membershipRepository.FindAsync(userId, tenantId, cancellationToken);
        if (membership is null)
        {
            membership = new IdentityUserTenantMembership(userId, tenantId);
            await membershipRepository.InsertAsync(membership, cancellationToken);
        }
        else
        {
            membership.Activate();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToRecord(membership);
    }

    public async ValueTask RevokeAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        EnsureHostContext();
        EnsureId(userId, nameof(userId), "User");
        EnsureId(tenantId, nameof(tenantId), "Tenant");

        var membership = await membershipRepository.FindAsync(userId, tenantId, cancellationToken);
        if (membership is null)
        {
            return;
        }

        membership.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask<UserTenantMembershipRecord?> FindAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        EnsureId(userId, nameof(userId), "User");
        EnsureId(tenantId, nameof(tenantId), "Tenant");
        tenantScopeAccessPolicy.EnsureCanAccess(tenantId);

        var membership = await membershipRepository.FindAsync(userId, tenantId, cancellationToken);
        return membership is null ? null : MapToRecord(membership);
    }

    public async ValueTask<bool> IsActiveMemberAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        EnsureId(userId, nameof(userId), "User");
        EnsureId(tenantId, nameof(tenantId), "Tenant");

        var membership = await membershipRepository.FindAsync(userId, tenantId, cancellationToken);
        return membership?.IsActive == true;
    }

    public async ValueTask<IReadOnlyList<UserTenantMembershipRecord>> GetListByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        EnsureId(userId, nameof(userId), "User");

        var memberships = await membershipRepository.GetListByUserIdAsync(userId, cancellationToken);
        return tenantScopeAccessPolicy
            .FilterAccessible(memberships.Select(MapToRecord), membership => membership.TenantId)
            .OrderBy(membership => membership.TenantId)
            .ToArray();
    }

    private void EnsureHostContext()
    {
        if (currentTenant.Id is not null)
        {
            throw new UnauthorizedAccessException("Tenant membership management is only available in host context.");
        }
    }

    private async ValueTask EnsureTenantIsActiveAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var tenant = await tenantResolutionStore.FindByIdAsync(tenantId, cancellationToken);
        if (tenant is null)
        {
            throw new BusinessException(
                $"Tenant '{tenantId}' was not found.",
                new ErrorCode("Identity.TenantNotFound"));
        }

        if (!tenant.IsActive)
        {
            throw new BusinessException(
                $"Tenant '{tenant.Name}' is inactive and cannot be assigned.",
                new ErrorCode("Identity.InactiveTenant"));
        }
    }

    private static UserTenantMembershipRecord MapToRecord(IdentityUserTenantMembership membership)
        => new(membership.UserId, membership.TenantId, membership.IsActive);

    private static void EnsureId(Guid id, string parameterName, string label)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException($"{label} id cannot be empty.", parameterName);
        }
    }
}
