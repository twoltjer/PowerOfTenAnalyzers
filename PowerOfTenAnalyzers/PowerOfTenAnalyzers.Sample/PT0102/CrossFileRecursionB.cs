namespace PowerOfTenAnalyzers.Sample.PT0102;

public class CrossFileRecursionB
{
    public static void Method()
    {
        CrossFileRecursionA.Method();
    }
}