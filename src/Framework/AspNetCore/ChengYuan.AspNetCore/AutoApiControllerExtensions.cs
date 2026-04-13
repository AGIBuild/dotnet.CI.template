using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace ChengYuan.AspNetCore;

public static class AutoApiControllerExtensions
{
    public static IMvcBuilder AddAutoApiControllers(
        this IMvcBuilder mvcBuilder,
        Action<AutoApiControllerOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(mvcBuilder);

        var options = new AutoApiControllerOptions();
        configure?.Invoke(options);

        mvcBuilder.ConfigureApplicationPartManager(manager =>
        {
            manager.FeatureProviders.Add(new AutoApiControllerFeatureProvider(options));
        });

        mvcBuilder.Services.Configure<MvcOptions>(mvcOptions =>
        {
            mvcOptions.Conventions.Add(new AutoApiControllerConvention(options));
        });

        return mvcBuilder;
    }
}
