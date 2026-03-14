namespace Dotnet.CI.Template.Sample;

/// <summary>Provides basic arithmetic operations.</summary>
public static class Calculator
{
    /// <summary>Returns the sum of two integers.</summary>
    public static int Add(int left, int right) => left + right;

    /// <summary>Returns the integer quotient of <paramref name="dividend"/> divided by <paramref name="divisor"/>.</summary>
    /// <exception cref="DivideByZeroException">Thrown when <paramref name="divisor"/> is zero.</exception>
    public static int Divide(int dividend, int divisor)
    {
        if (divisor == 0)
            throw new DivideByZeroException();

        return dividend / divisor;
    }
}
