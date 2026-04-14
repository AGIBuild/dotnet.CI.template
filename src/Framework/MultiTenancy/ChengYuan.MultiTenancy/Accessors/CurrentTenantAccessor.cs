using System;
using System.Threading;
using ChengYuan.Core;

namespace ChengYuan.MultiTenancy;

internal sealed class CurrentTenantAccessor : ICurrentTenantAccessor
{
    private readonly AsyncLocal<TenantInfo?> _currentTenant = new();

    public Guid? Id => _currentTenant.Value?.Id;

    public string? Name => _currentTenant.Value?.Name;

    public bool IsAvailable => Id.HasValue;

    public IDisposable Change(Guid? tenantId, string? tenantName = null)
    {
        var previousTenant = _currentTenant.Value;
        _currentTenant.Value = new TenantInfo(tenantId, tenantName);
        return new DelegateDisposable(() => _currentTenant.Value = previousTenant);
    }
}
