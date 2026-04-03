using ChengYuan.Core.Lifecycle;
using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.Core.Data;

public sealed class DataModule : ModuleBase, IPreConfigureServices
{
    public void PreConfigureServices(IServiceCollection services)
    {
        services.TryAddSingleton(typeof(IDataFilter<>), typeof(DataFilter<>));
        services.TryAddSingleton<IDataTenantProvider, NullDataTenantProvider>();
        services.TryAddSingleton<IUnitOfWorkAccessor, UnitOfWorkAccessor>();
    }
}
