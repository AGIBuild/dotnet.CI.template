using ChengYuan.Core.Modularity;
using ChengYuan.EntityFrameworkCore;

namespace ChengYuan.EntityFrameworkCore.Sqlite;

[DependsOn(typeof(ChengYuanEntityFrameworkCoreModule))]
public sealed class ChengYuanEntityFrameworkCoreSqliteModule : ExtensionModule
{
    protected override void ValidateExtensionAttachment(IModuleLoadContext context, IModuleDescriptor attachedCapability)
    {
        if (attachedCapability.ModuleType != typeof(ChengYuanEntityFrameworkCoreModule))
        {
            throw new InvalidOperationException(
                $"SQLite EF Core module must attach to '{typeof(ChengYuanEntityFrameworkCoreModule).FullName}', but attached to '{attachedCapability.ModuleType.FullName}'.");
        }
    }
}
