using System.Text.RegularExpressions;

namespace BicepFlex.Tokens;

public class BicepScopeToken : BicepToken
{
    private static readonly Regex BicepCommentRegex = new(@"^\s*targetScope\s*\=\s*\'(\w+)\'.*$");
    public string Scope { get; }

    private BicepScopeToken(string scope)
    {
        Scope = scope;
    }

    public static bool TryParse(IEnumerator<string> reader, out BicepScopeToken? token)
    {
        var line = reader.Current;
        var match = BicepCommentRegex.Match(line);
        if (match.Success)
        {
            token = new BicepScopeToken(match.Groups[1].Value);
            return true;
        }

        token = null;
        return false;
    }
}