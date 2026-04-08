using System;
using ChengYuan.Core.Data;

namespace ChengYuan.MultiTenancy;

internal sealed class CurrentTenantDataTenantProvider(ICurrentTenant currentTenant) : IDataTenantProvider
{
    public Guid? TenantId => currentTenant.Id;

    public bool IsAvailable => currentTenant.IsAvailable;
}
