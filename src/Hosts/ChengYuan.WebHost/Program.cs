using System.Linq;
using ChengYuan.Core.Modularity;
using ChengYuan.ExecutionContext;
using ChengYuan.MultiTenancy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

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

public partial class Program
{
}
