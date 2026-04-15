using System;
using System.Collections.Concurrent;
using System.Reflection;
using ChengYuan.Core.Data;
using ChengYuan.Core.Data.Auditing;
using Microsoft.Extensions.Options;

namespace ChengYuan.Auditing;

internal sealed class AuditableEntityTypeResolver(IOptions<AuditingOptions> options) : IAuditableEntityTypeResolver
{
    private readonly AuditingOptions _options = options.Value;
    private readonly ConcurrentDictionary<Type, bool> _entityTypeCache = new();
    private readonly ConcurrentDictionary<(Type, string), bool> _propertyCache = new();

    public bool IsAuditable(Type entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);

        return _entityTypeCache.GetOrAdd(entityType, ResolveEntityAuditability);
    }

    public bool IsPropertyAuditable(Type entityType, string propertyName)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);

        return _propertyCache.GetOrAdd((entityType, propertyName), static (key, _) =>
        {
            var property = key.Item1.GetProperty(key.Item2, BindingFlags.Public | BindingFlags.Instance);
            return property?.GetCustomAttribute<DisableAuditingAttribute>() is null;
        }, (object?)null);
    }

    private bool ResolveEntityAuditability(Type entityType)
    {
        if (entityType.GetCustomAttribute<DisableAuditingAttribute>() is not null)
        {
            return false;
        }

        if (entityType.GetCustomAttribute<AuditedAttribute>() is not null)
        {
            return true;
        }

        if (typeof(ICreationAudited).IsAssignableFrom(entityType)
            || typeof(IModificationAudited).IsAssignableFrom(entityType))
        {
            return true;
        }

        foreach (var selector in _options.EntityHistorySelectors)
        {
            if (selector(entityType))
            {
                return true;
            }
        }

        return false;
    }
}
