using System;

namespace ChengYuan.MultiTenancy;

public interface ICurrentTenantAccessor : ICurrentTenant
{
    IDisposable Change(Guid? tenantId, string? tenantName = null);
}
