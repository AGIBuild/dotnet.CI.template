using ChengYuan.Core.Lifecycle;
using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.Core.Validation;

[DependsOn(typeof(global::ChengYuan.Core.CoreRuntimeModule))]
public sealed class ValidationModule : ModuleBase, IPreConfigureServices
{
    public void PreConfigureServices(IServiceCollection services)
    {
        services.TryAddTransient(typeof(IObjectValidator<>), typeof(ObjectValidator<>));
    }
}
