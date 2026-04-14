namespace ChengYuan.Core.Data.Auditing;

public sealed class EntityChangeInfo
{
    public required string EntityTypeFullName { get; init; }

    public required string? EntityId { get; init; }

    public required EntityChangeType ChangeType { get; init; }

    public IReadOnlyList<PropertyChangeInfo> PropertyChanges { get; init; } = [];
}
