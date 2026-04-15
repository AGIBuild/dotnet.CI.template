using System;

namespace ChengYuan.Core.Data.Auditing;

public sealed class NullAuditableEntityTypeResolver : IAuditableEntityTypeResolver
{
    public static NullAuditableEntityTypeResolver Instance { get; } = new();

    public bool IsAuditable(Type entityType) => false;

    public bool IsPropertyAuditable(Type entityType, string propertyName) => true;
}
