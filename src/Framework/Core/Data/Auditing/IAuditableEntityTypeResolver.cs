using System;

namespace ChengYuan.Core.Data.Auditing;

public interface IAuditableEntityTypeResolver
{
    bool IsAuditable(Type entityType);

    bool IsPropertyAuditable(Type entityType, string propertyName);
}
