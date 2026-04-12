using ChengYuan.Core.Modularity;
using ChengYuan.EntityFrameworkCore;

namespace ChengYuan.EntityFrameworkCore.PostgreSql;

[DependsOn(typeof(ChengYuanEntityFrameworkCoreModule))]
public sealed class ChengYuanEntityFrameworkCorePostgreSqlModule : ExtensionModule
{
    protected override void ValidateExtensionAttachment(IModuleLoadContext context, IModuleDescriptor attachedCapability)
    {
        if (attachedCapability.ModuleType != typeof(ChengYuanEntityFrameworkCoreModule))
        {
            throw new InvalidOperationException(
                $"PostgreSQL EF Core module must attach to '{typeof(ChengYuanEntityFrameworkCoreModule).FullName}', but attached to '{attachedCapability.ModuleType.FullName}'.");
        }
    }
}
