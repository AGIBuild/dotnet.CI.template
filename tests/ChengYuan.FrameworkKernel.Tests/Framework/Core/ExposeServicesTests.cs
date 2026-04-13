using System.Reflection;
using ChengYuan.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace ChengYuan.FrameworkKernel.Tests.Framework.Core;

public sealed class ExposeServicesTests
{
    [Fact]
    public void Class_with_ExposeServices_registers_all_declared_types()
    {
        var services = new ServiceCollection();
        services.AddConventionalServices(typeof(MultiInterfaceService).Assembly);

        var provider = services.BuildServiceProvider();

        provider.GetService<IServiceA>().ShouldNotBeNull();
        provider.GetService<IServiceB>().ShouldNotBeNull();
    }

    [Fact]
    public void ExposeServices_overrides_naming_convention()
    {
        var services = new ServiceCollection();
        services.AddConventionalServices(typeof(NameMismatchService).Assembly);

        var provider = services.BuildServiceProvider();

        // IFoo would NOT match NameMismatchService by naming convention,
        // but ExposeServices explicitly lists it.
        provider.GetService<IFoo>().ShouldNotBeNull();
    }

    public interface IServiceA;
    public interface IServiceB;
    public interface IFoo;

    [ExposeServices(typeof(IServiceA), typeof(IServiceB))]
    public sealed class MultiInterfaceService : IServiceA, IServiceB, IScopedService;

    [ExposeServices(typeof(IFoo))]
    public sealed class NameMismatchService : IFoo, ITransientService;
}
