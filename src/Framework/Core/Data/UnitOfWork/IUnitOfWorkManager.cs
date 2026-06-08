namespace ChengYuan.Core.Data;

public interface IUnitOfWorkManager
{
    IUnitOfWork? Current { get; }

    IUnitOfWork Begin(UnitOfWorkOptions? options = null);
}
