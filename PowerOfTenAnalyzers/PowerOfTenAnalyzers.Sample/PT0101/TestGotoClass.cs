namespace PowerOfTenAnalyzers.Sample.PT0101;

public class TestGotoClass
{
    public void TestGotoMethod()
    {
        goto MyLabel;
        
        MyLabel:
        return;
    }

    public string TestGotoSwitchMethod(Color color)
    {
        switch (color)
        {
            case Color.Red:
                goto case Color.Green;
            case Color.Blue:
                goto default;
            case Color.Green:
                return "Christmas color";
            default:
                return "Not Christmas color";
        }
    }

    public enum Color
    {
        Red,
        Blue,
        Green,
    }
}