namespace ChengYuan.Core.Modularity;

public interface IModuleManager
{
    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task ShutdownAsync(CancellationToken cancellationToken = default);
}
