using System.Text.Json;
using ChengYuan.Core.Data.Auditing;

namespace ChengYuan.AuditLogging;

public sealed class AuditLogEntityChangeEntity
{
    private string _entityTypeFullName = string.Empty;
    private string _serializedPropertyChanges = "[]";

    private AuditLogEntityChangeEntity()
    {
    }

    public AuditLogEntityChangeEntity(
        Guid id,
        Guid auditLogId,
        string entityTypeFullName,
        string? entityId,
        EntityChangeType changeType,
        DateTimeOffset changeTime,
        IReadOnlyList<PropertyChangeInfo> propertyChanges)
    {
        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        AuditLogId = auditLogId;
        EntityId = entityId;
        ChangeType = changeType;
        ChangeTime = changeTime;
        SetEntityTypeFullName(entityTypeFullName);
        SetSerializedPropertyChanges(propertyChanges);
    }

    public Guid Id { get; private set; }

    public Guid AuditLogId { get; private set; }

    public string EntityTypeFullName
    {
        get => _entityTypeFullName;
        private set => _entityTypeFullName = value;
    }

    public string? EntityId { get; private set; }

    public EntityChangeType ChangeType { get; private set; }

    public DateTimeOffset ChangeTime { get; private set; }

    public string SerializedPropertyChanges
    {
        get => _serializedPropertyChanges;
        private set => _serializedPropertyChanges = value;
    }

    private void SetEntityTypeFullName(string entityTypeFullName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityTypeFullName);
        EntityTypeFullName = entityTypeFullName;
    }

    private void SetSerializedPropertyChanges(IReadOnlyList<PropertyChangeInfo> propertyChanges)
    {
        ArgumentNullException.ThrowIfNull(propertyChanges);
        SerializedPropertyChanges = JsonSerializer.Serialize(propertyChanges);
    }
}
