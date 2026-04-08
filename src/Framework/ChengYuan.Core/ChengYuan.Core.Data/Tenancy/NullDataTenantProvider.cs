using System;

namespace ChengYuan.Core.Data;

public sealed class NullDataTenantProvider : IDataTenantProvider
{
    public Guid? TenantId => null;

    public bool IsAvailable => false;
}
