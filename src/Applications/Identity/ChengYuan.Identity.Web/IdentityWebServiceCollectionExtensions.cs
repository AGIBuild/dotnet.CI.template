using ChengYuan.AspNetCore;
using ChengYuan.Core.UI.Navigation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ChengYuan.Identity;

public static class IdentityWebServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityWeb(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<JwtOptions>()
            .BindConfiguration("Jwt")
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<JwtTokenService>();
        services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();

        services.AddTransient<IEndpointContributor, IdentityEndpointContributor>();
        services.AddTransient<IMenuContributor, IdentityMenuContributor>();

        return services;
    }
}
