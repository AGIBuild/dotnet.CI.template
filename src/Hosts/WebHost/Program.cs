using ChengYuan.EntityFrameworkCore;
using ChengYuan.WebHost;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: System.Globalization.CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

    var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=chengyuan-webhost.db";
    builder.Services.UseSqlite(connectionString);
    builder.Services.AddWebHostComposition();

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    app.UseWebHostComposition();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program
{
}
