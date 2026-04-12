using ChengYuan.Caching;
using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;

namespace ChengYuan.CliHost;

[DependsOn(typeof(ExecutionContextModule))]
[DependsOn(typeof(MultiTenancyModule))]
[DependsOn(typeof(MemoryCachingModule))]
internal sealed class CliHostFrameworkCompositionModule : HostModule
{
}