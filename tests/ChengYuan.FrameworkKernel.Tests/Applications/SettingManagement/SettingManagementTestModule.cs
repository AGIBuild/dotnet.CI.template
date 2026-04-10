using ChengYuan.Core.Modularity;
using ChengYuan.SettingManagement;

namespace ChengYuan.FrameworkKernel.Tests;

[DependsOn(typeof(SettingManagementModule))]
internal sealed class SettingManagementTestModule : ModuleBase
{
}
