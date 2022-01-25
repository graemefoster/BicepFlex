using System.Text.RegularExpressions;

namespace BicepFlex;

public class BicepParameterToken : BicepToken
{
    private static readonly Regex bicepParameterRegex =
        new(@"^\s*param\s+([A-Za-z0-9_]*?)\s+([A-Za-z0-9_]*?)(\s*|\s+.*?)$");

    private static readonly Regex bicepTypeRegex = new(@"\/\/.*?@bicepflextype\s+([A-Za-z0-9_\.]*)(\s*|(\s+.*?))$");

    public BicepParameterToken(string name, string bicepType, string? customType)
    {
        Name = name;
        BicepType = bicepType;
        CustomType = customType;
    }

    public string Name { get; set; }
    public string BicepType { get; set; }
    public string? CustomType { get; set; }

    public static bool TryParse(IEnumerator<string> reader, out BicepParameterToken token)
    {
        var line = reader.Current;
        var match = bicepParameterRegex.Match(line);
        if (match.Success)
        {
            var typeMatch = bicepTypeRegex.Match(line);
            token = new BicepParameterToken(
                match.Groups[1].Value,
                match.Groups[2].Value,
                typeMatch.Success ? typeMatch.Groups[1].Value : null
            );
            return true;
        }

        token = null;
        return false;
    }

    public string DotNetTypeName()
    {
        if (CustomType == null) return BicepType == "array" ? "System.Array" : BicepType;

        if (BicepType == "array") return $"{CustomType}[]";

        return CustomType;
    }
}