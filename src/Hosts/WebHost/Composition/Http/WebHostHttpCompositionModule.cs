using System.Threading.RateLimiting;
using Asp.Versioning;
using ChengYuan.AspNetCore;
using ChengYuan.AspNetCore.Configuration;
using ChengYuan.Authorization;
using ChengYuan.Core.Json;
using ChengYuan.Core.Modularity;
using ChengYuan.ExceptionHandling;
using ChengYuan.HealthChecks;
using ChengYuan.MultiTenancy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace ChengYuan.WebHost;

[DependsOn(typeof(MultiTenancyModule))]
[DependsOn(typeof(ExceptionHandlingModule))]
internal sealed class WebHostHttpCompositionModule : HostModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpContextAccessor();

        context.Services.ConfigureHttpJsonOptions(httpJsonOptions =>
        {
            var sharedOptions = new ChengYuanJsonOptions().JsonSerializerOptions;
            foreach (var converter in sharedOptions.Converters)
            {
                httpJsonOptions.SerializerOptions.Converters.Add(converter);
            }
        });

        context.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        context.Services.AddAuthorization();
        context.Services.AddChengYuanAuthorization();

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

        context.Services.TryAddTransient<IApplicationConfigurator, DefaultApplicationConfigurator>();
    }
}
