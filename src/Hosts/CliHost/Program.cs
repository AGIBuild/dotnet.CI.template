using ChengYuan.CliHost;
using ChengYuan.EntityFrameworkCore;
using ChengYuan.Hosting;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.UseSqlite("Data Source=chengyuan-clihost.db");
builder.AddChengYuan<CliHostModule>(cy => cy
    .DisableMultiTenancy()
);

await builder.Build().RunAsync();
