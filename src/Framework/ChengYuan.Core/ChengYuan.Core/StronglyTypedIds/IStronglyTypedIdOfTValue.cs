namespace ChengYuan.Core.StronglyTypedIds;

public interface IStronglyTypedId<out TValue>
{
    TValue Value { get; }
}
