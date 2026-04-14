using ChengYuan.Core.Exceptions;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class BusinessExceptionTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new BusinessException("Something failed", new ErrorCode("ERR001"), inner)
        {
            Details = "Detail info",
            LogLevel = LogLevel.Error,
        };

        ex.ErrorCode!.Value.Value.ShouldBe("ERR001");
        ex.Message.ShouldBe("Something failed");
        ex.Details.ShouldBe("Detail info");
        ex.InnerException.ShouldBe(inner);
        ex.LogLevel.ShouldBe(LogLevel.Error);
    }

    [Fact]
    public void Constructor_Defaults_LogLevelToWarning()
    {
        var ex = new BusinessException("fail", new ErrorCode("ERR"));

        ex.LogLevel.ShouldBe(LogLevel.Warning);
        ex.Details.ShouldBeNull();
    }

    [Fact]
    public void WithData_ChainsFluentlyAndStoresData()
    {
        var ex = new BusinessException("fail", new ErrorCode("ERR002"))
            .WithData("userId", 42)
            .WithData("action", "delete");

        ex.Data["userId"].ShouldBe(42);
        ex.Data["action"].ShouldBe("delete");
    }

    [Fact]
    public void ToResultError_ReturnsCorrectResult()
    {
        var ex = new BusinessException("fail", new ErrorCode("ERR003"));

        var error = ex.ToResultError();

        error.Code.ShouldBe("ERR003");
        error.Message.ShouldBe("fail");
    }
}
