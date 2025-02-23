namespace PowerOfTenAnalyzers.Sample.PT0102;

public class SimpleConstructorRecursion
{
    public SimpleConstructorRecursion(int value)
    {
        if (value % 2 == 0)
        {
            // ReSharper disable once UnusedVariable
            var inner = new SimpleConstructorRecursion(4);
        }
    }
}