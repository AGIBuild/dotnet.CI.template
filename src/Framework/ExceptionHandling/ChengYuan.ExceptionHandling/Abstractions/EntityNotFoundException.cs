using System;
using ChengYuan.Core.Exceptions;

namespace ChengYuan.ExceptionHandling;

public sealed class EntityNotFoundException : ChengYuanException
{
    public EntityNotFoundException(Type entityType, object? id = null)
        : base(
            id is not null
                ? $"Entity of type '{entityType.FullName}' with id '{id}' was not found."
                : $"Entity of type '{entityType.FullName}' was not found.",
            new ErrorCode("ChengYuan.EntityNotFound"))
    {
        EntityType = entityType;
        EntityId = id;
    }

    public Type EntityType { get; }

    public object? EntityId { get; }
}
