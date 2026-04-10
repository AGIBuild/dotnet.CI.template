using System;
using System.Collections.Generic;

namespace ChengYuan.Core.Entities;

public abstract class Entity<TId> : IEntity<TId>, IEquatable<Entity<TId>>
    where TId : notnull
{
    protected Entity()
    {
        Id = default!;
    }

    protected Entity(TId id)
    {
        SetId(id);
    }

    public TId Id { get; protected set; } = default!;

    public bool Equals(Entity<TId>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        if (HasDefaultId() || other.HasDefaultId())
        {
            return false;
        }

        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HasDefaultId()
            ? base.GetHashCode()
            : HashCode.Combine(GetType(), Id);
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !Equals(left, right);
    }

    protected void SetId(TId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        Id = id;
    }

    private bool HasDefaultId()
    {
        return EqualityComparer<TId>.Default.Equals(Id, default!);
    }
}
