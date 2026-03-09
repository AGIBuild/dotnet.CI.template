namespace Dotnet.CI.Template.Sample;

public sealed class Calculator
{
    public int Add(int left, int right) => left + right;

    public int Divide(int dividend, int divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException();

        return dividend / divisor;
    }
}
