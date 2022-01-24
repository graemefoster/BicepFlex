namespace BicepFlex;

public class BicepEnumToken : BicepToken
{
    public static bool TryParse(IEnumerator<string> reader, out BicepToken token)
    {
        var line = reader.Current;
        if (line.Contains("@allowed(["))
        {
            token = new BicepEnumToken();
            return true;
        }

        token = null;
        return false;
    }
}