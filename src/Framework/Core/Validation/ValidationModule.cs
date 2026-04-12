using ChengYuan.Core.Lifecycle;
using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChengYuan.Core.Validation;

[DependsOn(typeof(global::ChengYuan.Core.CoreRuntimeModule))]
public sealed class ValidationModule : FrameworkCoreModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.TryAddTransient(typeof(IObjectValidator<>), typeof(ObjectValidator<>));
    }
}
