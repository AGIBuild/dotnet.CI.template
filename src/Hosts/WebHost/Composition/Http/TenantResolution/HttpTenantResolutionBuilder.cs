using ChengYuan.MultiTenancy;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.WebHost;

internal sealed class HttpTenantResolutionBuilder(IServiceCollection services)
{
    public HttpTenantResolutionBuilder AddDefaultSources()
    {
        return AddClaimSource()
            .AddHeaderSource()
            .AddQueryStringSource()
            .AddRouteSource()
            .AddCookieSource()
            .AddDomainSource();
    }

    public HttpTenantResolutionBuilder AddClaimSource()
        => AddSource<ClaimTenantResolutionSource>();

    public HttpTenantResolutionBuilder AddHeaderSource()
        => AddSource<HeaderTenantResolutionSource>();

    public HttpTenantResolutionBuilder AddQueryStringSource()
        => AddSource<QueryStringTenantResolutionSource>();

    public HttpTenantResolutionBuilder AddRouteSource()
        => AddSource<RouteTenantResolutionSource>();

    public HttpTenantResolutionBuilder AddCookieSource()
        => AddSource<CookieTenantResolutionSource>();

    public HttpTenantResolutionBuilder AddDomainSource()
        => AddSource<DomainTenantResolutionSource>();

    private HttpTenantResolutionBuilder AddSource<TSource>()
        where TSource : class, ITenantResolutionSource
    {
        services.AddSingleton<ITenantResolutionSource>(serviceProvider =>
            ActivatorUtilities.CreateInstance<TSource>(
                serviceProvider,
                serviceProvider.GetRequiredService<MultiTenancyOptions>()));

        return this;
    }
}