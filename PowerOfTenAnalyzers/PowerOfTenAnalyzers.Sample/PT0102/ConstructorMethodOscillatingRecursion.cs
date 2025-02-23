using System;

namespace PowerOfTenAnalyzers.Sample.PT0102;

public class ConstructorMethodOscillatingRecursion
{
    public ConstructorMethodOscillatingRecursion()
    {
        Method();
    }

    private void Method()
    {
        var left = new ConstructorMethodOscillatingRecursion();
        var right = new ConstructorMethodOscillatingRecursion();
        if (left == right)
            throw new InvalidOperationException();
    }
}