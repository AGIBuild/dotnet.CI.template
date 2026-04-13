using System.Threading.RateLimiting;
using Asp.Versioning;
using ChengYuan.AspNetCore;
using ChengYuan.Core.Modularity;
using ChengYuan.ExceptionHandling;
using ChengYuan.HealthChecks;
using ChengYuan.MultiTenancy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

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

        context.Services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            });

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

        context.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("ChengYuan.WebHost"))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddOtlpExporter());

        context.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("per-tenant", httpContext =>
            {
                var tenant = httpContext.RequestServices.GetService<ICurrentTenant>();
                var partitionKey = tenant?.Id?.ToString() ?? "host";
                return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                });
            });
        });
    }
}
