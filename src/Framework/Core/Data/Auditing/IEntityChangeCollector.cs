namespace ChengYuan.Core.Data.Auditing;

public interface IEntityChangeCollector
{
    bool IsActive { get; }

    void Collect(EntityChangeInfo changeInfo);
}
