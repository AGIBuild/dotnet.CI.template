using ChengYuan.Core.Modularity;
using ChengYuan.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.AspNetCore;

public static class ChengYuanWebBuilderExtensions
{
    /// <summary>
    /// Adds the ChengYuan modular framework to an ASP.NET Core host.
    /// The specified <typeparamref name="TWebRootModule"/> serves as the module graph root
    /// and should configure HTTP-layer services (auth, CORS, versioning, etc.).
    /// </summary>
    public static WebApplicationBuilder AddChengYuan<TWebRootModule>(
        this WebApplicationBuilder builder,
        Action<ChengYuanBuilder> configure)
        where TWebRootModule : HostModule, new()
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        var cy = new ChengYuanBuilder(builder.Services);
        configure(cy);
        cy.Apply();

        builder.Services.AddModularApplication<TWebRootModule>();
        builder.Services.AddHostedService<ModularApplicationHostedService>();
        return builder;
    }
}
