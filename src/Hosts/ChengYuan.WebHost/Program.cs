using ChengYuan.Caching;
using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddModule<WebHostModule>();

var app = builder.Build();

app.MapGet("/health", (ModuleCatalog catalog, ICurrentCorrelation correlation, ICurrentTenant currentTenant) =>
    Results.Ok(new
    {
        status = "ok",
        correlationId = correlation.CorrelationId,
        tenantId = currentTenant.Id,
        modules = catalog.ModuleTypes.Select(moduleType => moduleType.Name)
    }));

app.Run();

[DependsOn(typeof(ExecutionContextModule))]
[DependsOn(typeof(MultiTenancyModule))]
[DependsOn(typeof(MemoryCachingModule))]
internal sealed class WebHostModule : ModuleBase
{
}

public partial class Program
{
}
