namespace PowerOfTenAnalyzers.Sample.PT0102;

public class CrossFileRecursionA
{
    public static void Method()
    {
        CrossFileRecursionB.Method();
    }
}