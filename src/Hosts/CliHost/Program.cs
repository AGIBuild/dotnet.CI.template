using ChengYuan.CliHost;
using ChengYuan.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.UseSqlite("Data Source=chengyuan-clihost.db");
services.AddCliHostComposition();

await using var serviceProvider = services.BuildServiceProvider();
await serviceProvider.RunCliHostCompositionAsync();
