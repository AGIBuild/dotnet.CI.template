namespace ChengYuan.Core.Modularity;

public interface IModularApplication
{
    IServiceProvider Services { get; }

    IModuleCatalog ModuleCatalog { get; }

    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task ShutdownAsync(CancellationToken cancellationToken = default);
}
