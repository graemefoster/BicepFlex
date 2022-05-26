using System.Text.RegularExpressions;

namespace BicepParser.Tokens;

public class BicepCommentToken
{

    private static readonly Regex BicepCommentRegex = new(@"^\s*//.*$");

    public static bool TryParse(IEnumerator<string> reader)
    {
        var line = reader.Current;
        var match = BicepCommentRegex.Match(line);
        if (match.Success)
        {
            new BicepCommentToken();
            return true;
        }

        return false;
    }
}