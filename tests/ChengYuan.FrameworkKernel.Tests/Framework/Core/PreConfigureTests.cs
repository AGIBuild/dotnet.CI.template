using ChengYuan.Core.DependencyInjection;
using ChengYuan.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class PreConfigureTests
{
    [Fact]
    public void PreConfigure_ShouldAccumulateActions()
    {
        var services = new ServiceCollection();

        services.PreConfigure<TestPreConfigureOptions>(o => o.Value1 = "a");
        services.PreConfigure<TestPreConfigureOptions>(o => o.Value2 = "b");

        var result = services.ExecutePreConfiguredActions<TestPreConfigureOptions>();
        result.Value1.ShouldBe("a");
        result.Value2.ShouldBe("b");
    }

    [Fact]
    public void PreConfigure_ShouldApplyToExistingInstance()
    {
        var services = new ServiceCollection();
        var existing = new TestPreConfigureOptions { Value1 = "original" };

        services.PreConfigure<TestPreConfigureOptions>(o => o.Value2 = "added");

        var result = services.ExecutePreConfiguredActions(existing);
        result.Value1.ShouldBe("original");
        result.Value2.ShouldBe("added");
    }

    [Fact]
    public void PreConfigure_ShouldWorkAcrossModules()
    {
        var services = new ServiceCollection();
        services.AddModule<PreConfigureConsumerModule>();

        using var provider = services.BuildServiceProvider();
        var marker = provider.GetRequiredService<PreConfigureResultMarker>();

        marker.ComposedValue.ShouldBe("from-producer+from-consumer");
    }

    [Fact]
    public void PreConfigure_FromModuleBase_ShouldWork()
    {
        var services = new ServiceCollection();
        services.AddModule<ModuleBasePreConfigureTestModule>();

        using var provider = services.BuildServiceProvider();
        var marker = provider.GetRequiredService<ModuleBasePreConfigureMarker>();

        marker.Result.ShouldBe("via-module-base");
    }

    private sealed class TestPreConfigureOptions
    {
        public string? Value1 { get; set; }
        public string? Value2 { get; set; }
    }

    private sealed record PreConfigureResultMarker(string ComposedValue);

    private sealed class PreConfigureProducerOptions
    {
        public string Part1 { get; set; } = "";
        public string Part2 { get; set; } = "";
    }

    private sealed class PreConfigureProducerModule : ModuleBase
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.PreConfigure<PreConfigureProducerOptions>(o => o.Part1 = "from-producer");
        }
    }

    [DependsOn(typeof(PreConfigureProducerModule))]
    private sealed class PreConfigureConsumerModule : ModuleBase
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.PreConfigure<PreConfigureProducerOptions>(o => o.Part2 = "from-consumer");

            var result = context.Services.ExecutePreConfiguredActions<PreConfigureProducerOptions>();
            context.Services.AddSingleton(new PreConfigureResultMarker($"{result.Part1}+{result.Part2}"));
        }
    }

    private sealed record ModuleBasePreConfigureMarker(string Result);

    private sealed class ModuleBasePreConfigureHelperOptions
    {
        public string? Value { get; set; }
    }

    private sealed class ModuleBasePreConfigureTestModule : ModuleBase
    {
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            PreConfigure<ModuleBasePreConfigureHelperOptions>(o => o.Value = "via-module-base");
        }

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var result = context.Services.ExecutePreConfiguredActions<ModuleBasePreConfigureHelperOptions>();
            context.Services.AddSingleton(new ModuleBasePreConfigureMarker(result.Value!));
        }
    }
}
