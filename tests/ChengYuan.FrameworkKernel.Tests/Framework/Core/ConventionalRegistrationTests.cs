using ChengYuan.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class ConventionalRegistrationTests
{
    [Fact]
    public void AddConventionalServices_ShouldRegisterTransientByMarkerInterface()
    {
        var services = new ServiceCollection();
        services.AddConventionalServices(typeof(ConventionalRegistrationTests).Assembly);

        using var provider = services.BuildServiceProvider();
        var svc = provider.GetService<IConventionalTransientService>();

        svc.ShouldNotBeNull();
        svc.ShouldBeOfType<ConventionalTransientService>();
    }

    [Fact]
    public void AddConventionalServices_ShouldRegisterScopedByMarkerInterface()
    {
        var services = new ServiceCollection();
        services.AddConventionalServices(typeof(ConventionalRegistrationTests).Assembly);

        using var scope = services.BuildServiceProvider().CreateScope();
        var svc = scope.ServiceProvider.GetService<IConventionalScopedService>();

        svc.ShouldNotBeNull();
        svc.ShouldBeOfType<ConventionalScopedService>();
    }

    [Fact]
    public void AddConventionalServices_ShouldRegisterSingletonByMarkerInterface()
    {
        var services = new ServiceCollection();
        services.AddConventionalServices(typeof(ConventionalRegistrationTests).Assembly);

        using var provider = services.BuildServiceProvider();
        var svc1 = provider.GetRequiredService<IConventionalSingletonService>();
        var svc2 = provider.GetRequiredService<IConventionalSingletonService>();

        svc1.ShouldBeSameAs(svc2);
    }

    [Fact]
    public void AddConventionalServices_ShouldSkipDisabledTypes()
    {
        var services = new ServiceCollection();
        services.AddConventionalServices(typeof(ConventionalRegistrationTests).Assembly);

        using var provider = services.BuildServiceProvider();
        provider.GetService<IDisabledConventionalService>().ShouldBeNull();
    }

    [Fact]
    public void AddConventionalServices_ShouldNotOverrideExistingRegistrations()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConventionalSingletonService>(new ManualSingletonService());
        services.AddConventionalServices(typeof(ConventionalRegistrationTests).Assembly);

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IConventionalSingletonService>().ShouldBeOfType<ManualSingletonService>();
    }

    [Fact]
    public void AddConventionalServices_ShouldMatchByNamingConvention()
    {
        var services = new ServiceCollection();
        services.AddConventionalServices(typeof(ConventionalRegistrationTests).Assembly);

        using var provider = services.BuildServiceProvider();
        provider.GetService<IConventionalTransientService>().ShouldNotBeNull();
    }
}

public interface IConventionalTransientService
{
    string Name { get; }
}

public sealed class ConventionalTransientService : IConventionalTransientService, ITransientService
{
    public string Name => "transient";
}

public interface IConventionalScopedService;

public sealed class ConventionalScopedService : IConventionalScopedService, IScopedService;

public interface IConventionalSingletonService;

public sealed class ConventionalSingletonService : IConventionalSingletonService, ISingletonService;

public interface IDisabledConventionalService;

[DisableConventionalRegistration]
public sealed class DisabledConventionalService : IDisabledConventionalService, ITransientService;

public sealed class ManualSingletonService : IConventionalSingletonService;
