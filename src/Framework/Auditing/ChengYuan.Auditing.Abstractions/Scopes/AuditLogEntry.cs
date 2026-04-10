using System;
using System.Collections.Generic;

namespace ChengYuan.Auditing;

public sealed class AuditLogEntry
{
    private readonly Dictionary<string, object?> _properties = new(StringComparer.Ordinal);

    public AuditLogEntry(string name, DateTimeOffset startedAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        StartedAtUtc = startedAtUtc;
        Succeeded = true;
    }

    public string Name { get; }

    public DateTimeOffset StartedAtUtc { get; }

    public DateTimeOffset? CompletedAtUtc { get; set; }

    public TimeSpan? Duration { get; set; }

    public Guid? TenantId { get; set; }

    public string? UserId { get; set; }

    public string? UserName { get; set; }

    public bool IsAuthenticated { get; set; }

    public string? CorrelationId { get; set; }

    public bool Succeeded { get; set; }

    public string? ErrorMessage { get; set; }

    public IReadOnlyDictionary<string, object?> Properties => _properties;

    public bool TryGetProperty(string name, out object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _properties.TryGetValue(name, out value);
    }

    public void SetProperty(string name, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _properties[name] = value;
    }
}
