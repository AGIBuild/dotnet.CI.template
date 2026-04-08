using ChengYuan.Caching;
using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;

[DependsOn(typeof(ExecutionContextModule))]
[DependsOn(typeof(MultiTenancyModule))]
[DependsOn(typeof(MemoryCachingModule))]
internal sealed class WebHostModule : ModuleBase
{
}
