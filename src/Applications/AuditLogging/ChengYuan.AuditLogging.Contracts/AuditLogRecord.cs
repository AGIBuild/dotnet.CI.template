using System;
using System.Collections.Generic;

namespace ChengYuan.AuditLogging;

public sealed record AuditLogRecord
{
    public AuditLogRecord(
        string name,
        DateTimeOffset startedAtUtc,
        DateTimeOffset? completedAtUtc,
        TimeSpan? duration,
        Guid? tenantId,
        string? userId,
        string? userName,
        bool isAuthenticated,
        string? correlationId,
        bool succeeded,
        string? errorMessage,
        IReadOnlyDictionary<string, object?> properties)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(properties);

        Name = name;
        StartedAtUtc = startedAtUtc;
        CompletedAtUtc = completedAtUtc;
        Duration = duration;
        TenantId = tenantId;
        UserId = userId;
        UserName = userName;
        IsAuthenticated = isAuthenticated;
        CorrelationId = correlationId;
        Succeeded = succeeded;
        ErrorMessage = errorMessage;
        Properties = properties;
    }

    public string Name { get; }

    public DateTimeOffset StartedAtUtc { get; }

    public DateTimeOffset? CompletedAtUtc { get; }

    public TimeSpan? Duration { get; }

    public Guid? TenantId { get; }

    public string? UserId { get; }

    public string? UserName { get; }

    public bool IsAuthenticated { get; }

    public string? CorrelationId { get; }

    public bool Succeeded { get; }

    public string? ErrorMessage { get; }

    public IReadOnlyDictionary<string, object?> Properties { get; }

    public bool TryGetProperty(string name, out object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return Properties.TryGetValue(name, out value);
    }
}
