using ChengYuan.AspNetCore;
using ChengYuan.Core.Modularity;
using ChengYuan.ExceptionHandling;
using ChengYuan.HealthChecks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.WebHost;

[DependsOn(typeof(WebHostFrameworkCompositionModule))]
internal sealed class WebHostHttpCompositionModule : HostModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpContextAccessor();

        context.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        context.Services.AddAuthorization();

        context.Services.AddCors(options =>
            options.AddDefaultPolicy(policy =>
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

        new HttpTenantResolutionBuilder(context.Services)
            .AddDefaultSources();

        context.Services
            .AddControllers()
            .AddAutoValidation()
            .AddResultWrapper()
            .AddChengYuanExceptionFilter()
            .AddIdempotency();

        context.Services
            .AddHealthChecks()
            .AddChengYuanCacheCheck();

        context.Services.AddOpenApi();
    }
}
