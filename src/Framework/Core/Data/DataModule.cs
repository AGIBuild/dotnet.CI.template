using ChengYuan.Core.Lifecycle;
using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.Core.Data;

[DependsOn(typeof(global::ChengYuan.Core.CoreRuntimeModule))]
public sealed class DataModule : FrameworkCoreModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.TryAddSingleton(typeof(IDataFilter<>), typeof(DataFilter<>));
        context.Services.TryAddSingleton<IDataTenantProvider, NullDataTenantProvider>();
        context.Services.TryAddSingleton<IUnitOfWorkAccessor, UnitOfWorkAccessor>();
        context.Services.TryAddTransient<IDataSeeder, DataSeeder>();
        context.Services.TryAddSingleton<IDomainEventPublisher, NullDomainEventPublisher>();
        context.Services.TryAddTransient<IConnectionStringResolver, DefaultConnectionStringResolver>();
    }
}
