namespace Dotnet.CI.Template.Sample.Tests;

public class CalculatorTests
{
    [Fact]
    public void Add_ShouldReturnSum()
    {
        int result = Calculator.Add(2, 3);
        Assert.Equal(5, result);
    }

    [Fact]
    public void Divide_ByZero_ShouldThrow()
    {
        Assert.Throws<DivideByZeroException>(() => Calculator.Divide(10, 0));
    }
}
