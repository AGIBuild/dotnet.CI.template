using ChengYuan.Core.Data;
using ChengYuan.Core.Data.Auditing;
using ChengYuan.Core.Modularity;
using ChengYuan.Core.Timing;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.EntityFrameworkCore;

[DependsOn(typeof(DataModule))]
public sealed class ChengYuanEntityFrameworkCoreModule : FrameworkCoreModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddOptions<ChengYuanDbContextOptions>();

        context.Services.TryAddSingleton<IClock, DefaultClock>();
        context.Services.TryAddSingleton<IAuditUserProvider, NullAuditUserProvider>();
        context.Services.TryAddSingleton<IAuditableEntityTypeResolver, NullAuditableEntityTypeResolver>();
        context.Services.TryAddSingleton<IEntityChangeCollector, NullEntityChangeCollector>();

        context.Services.TryAddSingleton<AuditPropertySetterInterceptor>();
        context.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IInterceptor, AuditPropertySetterInterceptor>(
                sp => sp.GetRequiredService<AuditPropertySetterInterceptor>()));

        context.Services.TryAddSingleton<AuditEntityChangeInterceptor>();
        context.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IInterceptor, AuditEntityChangeInterceptor>(
                sp => sp.GetRequiredService<AuditEntityChangeInterceptor>()));

        context.Services.TryAddSingleton<ConcurrencyStampSaveChangesInterceptor>();
        context.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IInterceptor, ConcurrencyStampSaveChangesInterceptor>(
                sp => sp.GetRequiredService<ConcurrencyStampSaveChangesInterceptor>()));
    }
}
