using ChengYuan.Core.Modularity;
using ChengYuan.FeatureManagement;

namespace ChengYuan.FrameworkKernel.Tests;

[DependsOn(typeof(FeatureManagementModule))]
internal sealed class FeatureManagementTestModule : ModuleBase
{
}
