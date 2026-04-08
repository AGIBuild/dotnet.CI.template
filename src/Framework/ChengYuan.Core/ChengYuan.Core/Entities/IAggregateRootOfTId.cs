namespace ChengYuan.Core.Entities;

public interface IAggregateRoot<out TId> : IEntity<TId>, IAggregateRoot
    where TId : notnull
{
}
