namespace ChengYuan.MultiTenancy;

public sealed record TenantInfo(Guid? Id, string? Name);

public interface ICurrentTenant
{
    Guid? Id { get; }

    string? Name { get; }

    bool IsAvailable { get; }
}

public interface ICurrentTenantAccessor : ICurrentTenant
{
    IDisposable Change(Guid? tenantId, string? tenantName = null);
}
