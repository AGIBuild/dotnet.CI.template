using System;

namespace ChengYuan.ExceptionHandling;

public sealed class EntityNotFoundException : BusinessException
{
    public EntityNotFoundException(Type entityType, object? id = null)
        : base(
            code: "ChengYuan.EntityNotFound",
            message: id is not null
                ? $"Entity of type '{entityType.FullName}' with id '{id}' was not found."
                : $"Entity of type '{entityType.FullName}' was not found.")
    {
        EntityType = entityType;
        EntityId = id;
    }

    public Type EntityType { get; }

    public object? EntityId { get; }
}
