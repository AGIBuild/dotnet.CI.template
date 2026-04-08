using System;

namespace ChengYuan.MultiTenancy;

public interface ICurrentTenant
{
    Guid? Id { get; }

    string? Name { get; }

    bool IsAvailable { get; }
}
