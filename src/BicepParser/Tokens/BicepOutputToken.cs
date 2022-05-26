using System.Text.RegularExpressions;

namespace BicepParser.Tokens;

public class BicepOutputToken : BicepToken
{
    private static readonly Regex BicepOutputRegex = new(@"^\s*output\s+([A-Za-z0-9_]*?)\s+([A-Za-z0-9_]*?)\s*\=.*?");
    private static readonly Regex BicepTypeRegex = new(@"\/\/.*?@bicepflextype\s+([A-Za-z0-9_\.]*)(\s*|(\s+.*?))$");

    public BicepOutputToken(string name, string bicepType, string? customType)
    {
        Name = name;
        BicepType = bicepType;
        CustomType = customType;
    }

    public string BicepType { get; }
    public string? CustomType { get; }

    public string DotNetTypeName()
    {
        if (CustomType == null || CustomType == "array")
        {
            return BicepType switch
            {
                "array" => "System.Array",
                "resource" => "BicepResource",
                _ => BicepType
            };
        }
        if (BicepType == "array") return $"{CustomType}[]";

        return CustomType;
    }

    public static bool TryParse(IEnumerator<string> reader, out BicepOutputToken? token)
    {
        var line = reader.Current;
        var match = BicepOutputRegex.Match(line);
        if (match.Success)
        {
            var typeMatch = BicepTypeRegex.Match(line);
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