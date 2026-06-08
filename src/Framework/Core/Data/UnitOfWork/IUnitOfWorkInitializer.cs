namespace ChengYuan.Core.Data;

public interface IUnitOfWorkInitializer
{
    void Initialize(UnitOfWorkOptions options);
}
