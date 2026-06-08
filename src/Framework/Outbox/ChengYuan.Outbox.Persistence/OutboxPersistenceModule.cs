using ChengYuan.Core.Modularity;
using ChengYuan.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Outbox;

[DependsOn(typeof(OutboxModule))]
public sealed class OutboxPersistenceModule : ExtensionModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddConfiguredDbContext<OutboxDbContext>();
        context.Services.AddEntityFrameworkCoreDataAccess<OutboxDbContext>();
        context.Services.AddScoped<IOutboxStore, EfOutboxStore>();
    }
}
