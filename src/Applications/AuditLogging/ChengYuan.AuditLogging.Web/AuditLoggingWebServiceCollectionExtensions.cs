using ChengYuan.AspNetCore;
using ChengYuan.Core.UI.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.AuditLogging;

public static class AuditLoggingWebServiceCollectionExtensions
{
    public static IServiceCollection AddAuditLoggingWeb(this IServiceCollection services)
    {
        services.AddTransient<IEndpointContributor, AuditLoggingEndpointContributor>();
        services.AddTransient<IMenuContributor, AuditLoggingMenuContributor>();
        return services;
    }
}
