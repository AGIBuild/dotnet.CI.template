using ChengYuan.Core.DependencyInjection;
using ChengYuan.Core.Lifecycle;
using ChengYuan.Core.Modularity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class ModuleServiceRegistrationAsyncTests
{
    [Fact]
    public async Task AddModuleAsync_ShouldCallConfigureServicesAsync()
    {
        var services = new ServiceCollection();
        await services.AddModuleAsync<AsyncConfigureModule>();

        using var provider = services.BuildServiceProvider();
        var value = provider.GetRequiredService<AsyncConfiguredMarker>();

        value.ShouldNotBeNull();
        value.Value.ShouldBe("async-configured");
    }

    [Fact]
    public async Task AddModuleAsync_ShouldCallSyncConfigureServicesViaAsyncPath()
    {
        var services = new ServiceCollection();
        await services.AddModuleAsync<SyncOnlyConfigureModule>();

        using var provider = services.BuildServiceProvider();
        var value = provider.GetRequiredService<SyncConfiguredMarker>();

        value.ShouldNotBeNull();
    }

    [Fact]
    public async Task AddModuleAsync_ShouldWrapExceptionsWithModuleName()
    {
        var services = new ServiceCollection();

        var ex = await Should.ThrowAsync<InvalidOperationException>(
            services.AddModuleAsync<FailingAsyncConfigureModule>());

        ex.Message.ShouldContain(nameof(FailingAsyncConfigureModule));
        ex.InnerException.ShouldNotBeNull();
        ex.InnerException.Message.ShouldBe("async configure failure");
    }

    [Fact]
    public void ServiceConfigurationContext_ShouldExposeConfiguration_WhenHostRegistersIt()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["TestKey"] = "TestValue" })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddModule<ConfigurationReadingModule>();

        ConfigurationReadingModule.CapturedValue.ShouldBe("TestValue");
    }

    [Fact]
    public void ServiceConfigurationContext_ShouldHaveNullConfiguration_WhenHostDoesNotRegister()
    {
        var services = new ServiceCollection();
        services.AddModule<ConfigurationNullCheckModule>();

        ConfigurationNullCheckModule.WasNull.ShouldBeTrue();
    }

    [Fact]
    public async Task AddModularApplicationAsync_ShouldCallAsyncConfigureServices()
    {
        var services = new ServiceCollection();
        await services.AddModularApplicationAsync<AsyncConfigureModule>();

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<AsyncConfiguredMarker>().Value.ShouldBe("async-configured");
        provider.GetRequiredService<IModularApplication>().ShouldNotBeNull();
    }

    private sealed record AsyncConfiguredMarker(string Value);

    private sealed class AsyncConfigureModule : ModuleBase
    {
        public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
        {
            await Task.Yield();
            context.Services.AddSingleton(new AsyncConfiguredMarker("async-configured"));
        }
    }

    private sealed record SyncConfiguredMarker;

    private sealed class SyncOnlyConfigureModule : ModuleBase
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddSingleton(new SyncConfiguredMarker());
        }
    }

    private sealed class FailingAsyncConfigureModule : ModuleBase
    {
        public override async Task ConfigureServicesAsync(ServiceConfigurationContext context)
        {
            await Task.Yield();
            throw new InvalidOperationException("async configure failure");
        }
    }

    private sealed class ConfigurationReadingModule : ModuleBase
    {
        [ThreadStatic]
        internal static string? CapturedValue;

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            CapturedValue = context.Configuration?["TestKey"];
        }
    }

    private sealed class ConfigurationNullCheckModule : ModuleBase
    {
        [ThreadStatic]
        internal static bool WasNull;

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            WasNull = context.Configuration is null;
        }
    }
}
