using ChengYuan.AspNetCore;
using ChengYuan.AuditLogging;
using ChengYuan.BackgroundJobs;
using ChengYuan.EntityFrameworkCore;
using ChengYuan.FeatureManagement;
using ChengYuan.Identity;
using ChengYuan.PermissionManagement;
using ChengYuan.SettingManagement;
using ChengYuan.TenantManagement;
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

    var connectionString = builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException("Connection string 'Default' is not configured. Set it in appsettings.json under ConnectionStrings:Default.");

    builder.AddChengYuan<WebHostHttpCompositionModule>(cy => cy
        .UseSqlite(connectionString)
        .AddModule<IdentityWebModule>()
        .AddModule<TenantManagementWebModule>()
        .AddModule<SettingManagementWebModule>()
        .AddModule<PermissionManagementWebModule>()
        .AddModule<FeatureManagementWebModule>()
        .AddModule<AuditLoggingWebModule>()
        .AddModule<BackgroundJobPersistenceModule>()
    );

    builder.Services.AddDatabaseMigration();
    builder.Services.AddHostedService<DataSeedingHostedService>();

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
