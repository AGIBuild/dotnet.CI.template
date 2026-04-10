using ChengYuan.Caching;
using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddModule<CliHostModule>();

using var serviceProvider = services.BuildServiceProvider();

var catalog = serviceProvider.GetRequiredService<ModuleCatalog>();
var currentCorrelation = serviceProvider.GetRequiredService<ICurrentCorrelation>();

Console.WriteLine("ChengYuan CLI host");
Console.WriteLine($"Correlation: {currentCorrelation.CorrelationId}");
Console.WriteLine($"Loaded modules: {string.Join(", ", catalog.ModuleTypes.Select(moduleType => moduleType.Name))}");

[DependsOn(typeof(ExecutionContextModule))]
[DependsOn(typeof(MultiTenancyModule))]
[DependsOn(typeof(MemoryCachingModule))]
internal sealed class CliHostModule : ModuleBase
{
}
