using System.Text.RegularExpressions;

namespace BicepFlex.Tokens;

public class BicepOutputToken : BicepToken
{
    private static readonly Regex bicepOutputRegex = new(@"^\s*output\s+([A-Za-z0-9_]*?)\s+([A-Za-z0-9_]*?)\s*\=.*?");
    private static readonly Regex bicepTypeRegex = new(@"\/\/.*?@bicepflextype\s+([A-Za-z0-9_\.]*)(\s*|(\s+.*?))$");

    public BicepOutputToken(string name, string bicepType, string? customType)
    {
        Name = name;
        BicepType = bicepType;
        CustomType = customType;
    }

    public string BicepType { get; set; }
    public string? CustomType { get; set; }

    public string DotNetTypeName()
    {
        if (CustomType == null) return BicepType == "array" ? "System.Array" : BicepType;

        if (BicepType == "array") return $"{CustomType}[]";

        return CustomType;
    }

    public static bool TryParse(IEnumerator<string> reader, out BicepOutputToken? token)
    {
        var line = reader.Current;
        var match = bicepOutputRegex.Match(line);
        if (match.Success)
        {
            var typeMatch = bicepTypeRegex.Match(line);
            token = new BicepOutputToken(
                match.Groups[1].Value,
                match.Groups[2].Value,
                typeMatch.Success ? typeMatch.Groups[1].Value : null
            );
            return true;
        }

        token = null;
        return false;
    }
}