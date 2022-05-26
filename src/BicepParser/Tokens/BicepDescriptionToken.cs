using System.Text.RegularExpressions;

namespace BicepParser.Tokens;

public class BicepDescriptionToken
{

    private static readonly Regex BicepDescriptionRegex = new(@"^\s*@description\s*\(\s*'(.*?)'\s*\)\s*$");

    public BicepDescriptionToken(string description)
    {
        Description = description;
    }

    public string Description { get; }

    public static bool TryParse(IEnumerator<string> reader)
    {
        var line = reader.Current;
        var match = BicepDescriptionRegex.Match(line);
        if (match.Success)
        {
            new BicepDescriptionToken(
                match.Groups[1].Value
            );
            return true;
        }

        return false;
    }
}