using ChengYuan.Core.Modularity;
using ChengYuan.TenantManagement;

namespace ChengYuan.FrameworkKernel.Tests;

[DependsOn(typeof(TenantManagementModule))]
internal sealed class TenantManagementTestModule : ModuleBase
{
}
