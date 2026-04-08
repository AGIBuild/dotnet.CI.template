using System;

namespace ChengYuan.Core.Data;

public interface IDataTenantProvider
{
    Guid? TenantId { get; }

    bool IsAvailable { get; }
}
