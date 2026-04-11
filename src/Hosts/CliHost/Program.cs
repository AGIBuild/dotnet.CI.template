using ChengYuan.Caching;
using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddModularApplication<CliHostModule>();

using var serviceProvider = services.BuildServiceProvider();

var application = serviceProvider.GetRequiredService<IModularApplication>();
await application.InitializeAsync();

var catalog = application.ModuleCatalog;
var currentCorrelation = serviceProvider.GetRequiredService<ICurrentCorrelation>();

Console.WriteLine("ChengYuan CLI host");
Console.WriteLine($"Correlation: {currentCorrelation.CorrelationId}");
Console.WriteLine($"Loaded modules: {string.Join(", ", catalog.ModuleTypes.Select(moduleType => moduleType.Name))}");

await application.ShutdownAsync();
[DependsOn(typeof(ExecutionContextModule))]
[DependsOn(typeof(MemoryCachingModule))]
internal sealed class CliHostModule : ModuleBase
{
}
