using Microsoft.EntityFrameworkCore;

namespace ChengYuan.TenantManagement;

public sealed class EfTenantStore(TenantManagementDbContext dbContext) : ITenantStore
{
    public async ValueTask<TenantRecord?> FindByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id cannot be empty.", nameof(tenantId));
        }

        var tenant = await dbContext.Tenants
            .Where(tenant => !tenant.IsDeleted)
            .SingleOrDefaultAsync(tenant => tenant.Id == tenantId, cancellationToken);

        return tenant is null ? null : MapToRecord(tenant);
    }

    public async ValueTask<TenantRecord?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var normalizedName = TenantEntity.NormalizeName(name);
        var tenant = await dbContext.Tenants
            .Where(tenant => !tenant.IsDeleted)
            .SingleOrDefaultAsync(tenant => tenant.NormalizedName == normalizedName, cancellationToken);

        return tenant is null ? null : MapToRecord(tenant);
    }

    public async ValueTask<IReadOnlyList<TenantRecord>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var tenants = await dbContext.Tenants
            .Where(tenant => !tenant.IsDeleted)
            .OrderBy(tenant => tenant.NormalizedName)
            .ThenBy(tenant => tenant.Id)
            .Select(tenant => MapToRecord(tenant))
            .ToArrayAsync(cancellationToken);

        return tenants;
    }

    public async ValueTask SetAsync(TenantRecord tenant, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tenant);

        var normalizedName = TenantEntity.NormalizeName(tenant.Name);
        var duplicateTenantExists = await dbContext.Tenants
            .Where(entity => !entity.IsDeleted)
            .AnyAsync(entity => entity.NormalizedName == normalizedName && entity.Id != tenant.Id, cancellationToken);

        if (duplicateTenantExists)
        {
            throw new InvalidOperationException($"A tenant named '{tenant.Name}' already exists.");
        }

        var existingTenant = await dbContext.Tenants
            .SingleOrDefaultAsync(entity => entity.Id == tenant.Id, cancellationToken);

        if (existingTenant is null)
        {
            await dbContext.Tenants.AddAsync(new TenantEntity(tenant.Id, tenant.Name, tenant.IsActive), cancellationToken);
        }
        else
        {
            existingTenant.Update(tenant.Name, tenant.IsActive);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask RemoveAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("Tenant id cannot be empty.", nameof(tenantId));
        }

        var tenant = await dbContext.Tenants
            .SingleOrDefaultAsync(entity => entity.Id == tenantId, cancellationToken);

        if (tenant is null || tenant.IsDeleted)
        {
            return;
        }

        tenant.MarkDeleted();
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static TenantRecord MapToRecord(TenantEntity tenant)
    {
        return new TenantRecord(tenant.Id, tenant.Name, tenant.IsActive);
    }
}
