namespace PowerOfTenAnalyzers.Sample.PT0102;

public class SimpleMethodRecursion
{
    public SimpleMethodRecursion()
    {
        SimpleRecursiveMethod(4);
    }

    private void SimpleRecursiveMethod(int n)
    {
        if (n > 0)
            SimpleRecursiveMethod(n - 1);
    }
}