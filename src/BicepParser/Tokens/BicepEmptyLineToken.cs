using System.Text.RegularExpressions;

namespace BicepParser.Tokens;

public class BicepEmptyLineToken
{

    private static readonly Regex BicepEmptyLineRegex = new(@"^\s*$");

    public static bool TryParse(IEnumerator<string> reader)
    {
        var line = reader.Current;
        var match = BicepEmptyLineRegex.Match(line);
        if (match.Success)
        {
            new BicepEmptyLineToken();
            return true;
        }

        return false;
    }
}