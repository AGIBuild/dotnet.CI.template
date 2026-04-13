using ChengYuan.AspNetCore;
using Shouldly;

namespace ChengYuan.FrameworkKernel.Tests;

public sealed class ApiResponseTests
{
    [Fact]
    public void Ok_WithNoData_ReturnsSuccessResponse()
    {
        var response = ApiResponse.Ok();

        response.Success.ShouldBeTrue();
        response.Data.ShouldBeNull();
        response.Error.ShouldBeNull();
    }

    [Fact]
    public void Ok_WithData_IncludesDataInResponse()
    {
        var data = new { Id = 1, Name = "Test" };
        var response = ApiResponse.Ok(data);

        response.Success.ShouldBeTrue();
        response.Data.ShouldBe(data);
    }

    [Fact]
    public void Fail_IncludesErrorInfo()
    {
        var error = new ApiErrorInfo { Code = "ERR001", Message = "Something went wrong" };
        var response = ApiResponse.Fail(error);

        response.Success.ShouldBeFalse();
        response.Error.ShouldNotBeNull();
        response.Error.Code.ShouldBe("ERR001");
        response.Error.Message.ShouldBe("Something went wrong");
    }
}
