using ChengYuan.ExceptionHandling;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class BusinessExceptionTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new BusinessException("ERR001", "Something failed", "Detail info", inner, LogLevel.Error);

        ex.Code.ShouldBe("ERR001");
        ex.Message.ShouldBe("Something failed");
        ex.Details.ShouldBe("Detail info");
        ex.InnerException.ShouldBe(inner);
        ex.LogLevel.ShouldBe(LogLevel.Error);
    }

    [Fact]
    public void Constructor_Defaults_LogLevelToWarning()
    {
        var ex = new BusinessException();

        ex.LogLevel.ShouldBe(LogLevel.Warning);
        ex.Code.ShouldBeNull();
        ex.Details.ShouldBeNull();
    }

    [Fact]
    public void WithData_ChainsFluentlyAndStoresData()
    {
        var ex = new BusinessException("ERR002", "fail")
            .WithData("userId", 42)
            .WithData("action", "delete");

        ex.Data["userId"].ShouldBe(42);
        ex.Data["action"].ShouldBe("delete");
    }

    [Fact]
    public void Implements_InterfaceContracts()
    {
        var ex = new BusinessException("CODE", "msg", "details");

        (ex is IHasErrorCode).ShouldBeTrue();
        (ex is IHasErrorDetails).ShouldBeTrue();
        (ex is IHasLogLevel).ShouldBeTrue();
    }
}
