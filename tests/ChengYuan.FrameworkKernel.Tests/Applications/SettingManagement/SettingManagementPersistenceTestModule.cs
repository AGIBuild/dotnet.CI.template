using ChengYuan.Core.Modularity;
using ChengYuan.SettingManagement;

namespace ChengYuan.FrameworkKernel.Tests;

[DependsOn(typeof(SettingManagementPersistenceModule))]
internal sealed class SettingManagementPersistenceTestModule : ModuleBase
{
}
