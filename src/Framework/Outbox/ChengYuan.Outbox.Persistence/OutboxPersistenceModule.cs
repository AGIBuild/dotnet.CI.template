using ChengYuan.Core.Modularity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.Outbox;

[DependsOn(typeof(OutboxModule))]
public sealed class OutboxPersistenceModule : ExtensionModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hasDbContext = context.Services.Any(sd =>
            sd.ServiceType == typeof(DbContextOptions<OutboxDbContext>));

        if (hasDbContext)
        {
            context.Services.AddScoped<IOutboxStore, EfOutboxStore>();
        }
        else
        {
            context.Services.AddSingleton<IOutboxStore, InMemoryOutboxStore>();
        }
    }
}
