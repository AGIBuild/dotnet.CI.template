using Shouldly;

namespace Dotnet.CI.Template.Sample.Tests;

public class CalculatorTests
{
    [Fact]
    public void Add_ShouldReturnSum()
    {
        int result = Calculator.Add(2, 3);
        result.ShouldBe(5);
    }

    [Fact]
    public void Divide_ShouldReturnQuotient()
    {
        int result = Calculator.Divide(10, 2);
        result.ShouldBe(5);
    }

    [Fact]
    public void Divide_ByZero_ShouldThrow()
    {
        Should.Throw<DivideByZeroException>(() => Calculator.Divide(10, 0));
    }
}
