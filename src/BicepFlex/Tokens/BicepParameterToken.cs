using System.Reflection;
using System.Text.RegularExpressions;

namespace BicepFlex.Tokens;

public class BicepParameterToken : BicepToken
{

    private static readonly Regex BicepParameterRegex = new(@"^\s*param\s+([A-Za-z0-9_]*?)\s+([A-Za-z0-9_]*?)(\s*|\s+.*?)$");

    private static readonly Regex BicepTypeRegex = new(@"\/\/.*?@bicepflextype\s+([A-Za-z0-9_\.]*)(\s*|(\s+.*?))$");

    public BicepParameterToken(string name, string bicepType, string? customType)
    {
        Name = name;
        BicepType = bicepType;
        CustomType = customType;
    }

    public string BicepType { get; }
    public string? CustomType { get; internal set; }

    public static bool TryParse(string moduleName, IEnumerator<string> reader, out BicepParameterToken? token)
    {
        var line = reader.Current;
        var match = BicepParameterRegex.Match(line);
        if (match.Success)
        {
            var typeMatch = BicepTypeRegex.Match(line);
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
        if (CustomType == null || CustomType == "array") return BicepType == "array" ? "System.Array" : BicepType;

        if (BicepType == "array") return $"{CustomType}[]";

        return CustomType;
    }
}