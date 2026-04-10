using ChengYuan.Core.Modularity;
using ChengYuan.PermissionManagement;

namespace ChengYuan.FrameworkKernel.Tests;

[DependsOn(typeof(PermissionManagementModule))]
internal sealed class PermissionManagementTestModule : ModuleBase
{
}
