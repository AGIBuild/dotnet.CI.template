using System;

namespace ChengYuan.Core.Data;

public interface IMultiTenant
{
    Guid? TenantId { get; }
}
