using System.Globalization;
using System.Text.RegularExpressions;

namespace BicepFlex.Tokens;

public class BicepEnumToken : BicepParameterToken
{
    public BicepEnumToken(string moduleName, string name, string[] tokens) : base(name, "string", $"{moduleName.ToPascalCase()}{name.ToPascalCase()}Options")
    {
        Tokens = tokens;
    }

    public string[] Tokens { get; }

    public IEnumerable<(string DotNetFriendlyName, string BicepValue)> DotNetFriendlyTokens => Tokens.Select(DotNetFriendlyEnum);

    private (string, string) DotNetFriendlyEnum(string bicepValue)
    {
        if (bicepValue == "default")
        {
            return ("_default", bicepValue);
        }

        if (double.TryParse(bicepValue, out var val2))
        {
            return ($"NUM_{bicepValue.Replace(".", "_")}", bicepValue);
        }

        return (bicepValue, bicepValue);
    }

    public static bool TryParse(string moduleName, IEnumerator<string> reader, out BicepToken? token)
    {
        var tokenRegex = new Regex(@"^\s*'(.*)'\s*$");
        var line = reader.Current;
        if (line.Contains("@allowed(["))
        {
            var tokens = new List<string>();
            if (reader.MoveNext())
            {
                bool canRead;
                do
                {
                    if (reader.Current.Contains("])")) break;
                    var enumMatch = tokenRegex.Match(reader.Current);
                    if (enumMatch.Success)
                    {
                        tokens.Add(enumMatch.Groups[1].Value);
                    }

                    canRead = reader.MoveNext();
                } while (canRead);
            }
            else
            {
                throw new InvalidOperationException("Failed to read enum");
            }

            reader.MoveNext();
            if (BicepDescriptionToken.TryParse(reader)) reader.MoveNext();
            if (BicepCommentToken.TryParse(reader)) reader.MoveNext();
            if (BicepEmptyLineToken.TryParse(reader)) reader.MoveNext();

            if (BicepParameterToken.TryParse(moduleName, reader, out var parameterToken))
            {
                token = new BicepEnumToken(moduleName, parameterToken!.Name, tokens.ToArray());
                return true;
            }

            //Shouldn't happen
            throw new InvalidOperationException("Detected parameter token but failed to read");
        }

        token = null;
        return false;
    }
}