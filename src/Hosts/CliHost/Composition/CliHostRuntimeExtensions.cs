using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.CliHost;

public static class CliHostRuntimeExtensions
{
    public static async Task RunCliHostCompositionAsync(
        this IServiceProvider serviceProvider,
        TextWriter? output = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        output ??= Console.Out;

        var application = serviceProvider.GetRequiredService<IModularApplication>();
        await application.InitializeAsync(cancellationToken);

        try
        {
            var currentCorrelation = serviceProvider.GetRequiredService<ICurrentCorrelation>();
            var currentTenant = serviceProvider.GetRequiredService<ICurrentTenant>();

            await output.WriteLineAsync("ChengYuan CLI host");
            await output.WriteLineAsync($"Correlation: {currentCorrelation.CorrelationId}");
            await output.WriteLineAsync($"Tenant: {currentTenant.Id?.ToString() ?? "(none)"}");
            await output.WriteLineAsync($"Loaded modules: {string.Join(", ", application.ModuleCatalog.ModuleTypes.Select(moduleType => moduleType.Name))}");
        }
        finally
        {
            await application.ShutdownAsync(cancellationToken);
        }
    }
}