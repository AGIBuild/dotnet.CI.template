using ChengYuan.Core.Modularity;
using Microsoft.Extensions.Hosting;

namespace ChengYuan.Hosting;

public sealed class ModularApplicationHostedService(IModularApplication application) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return application.InitializeAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return application.ShutdownAsync(cancellationToken);
    }
}
