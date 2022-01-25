using System.Text.RegularExpressions;

namespace BicepFlex;

public class BicepEnumToken : BicepParameterToken
{
    public string[] Tokens { get; }

    public static bool TryParse(IEnumerator<string> reader, out BicepToken? token)
    {
        var tokenRegex = new Regex(@"^\s*'(.*)'\s*$");
        var line = reader.Current;
        if (line.Contains("@allowed(["))
        {
            var tokens = new List<string>();
            if (reader.MoveNext())
            {
                var canRead = true;
                do
                {
                    if (reader.Current.Contains("])")) break;
                    var enumMatch = tokenRegex.Match(reader.Current);
                    tokens.Add(enumMatch.Groups[1].Value);
                    canRead = reader.MoveNext();
                } while (canRead);
            }
            else
            {
                throw new InvalidOperationException("Failed to read enum");
            }

            reader.MoveNext();
            if (BicepParameterToken.TryParse(reader, out var parameterToken))
            {
                token = new BicepEnumToken(parameterToken.Name, tokens.ToArray());
                return true;
            }

            //Shouldn't happen
            throw new InvalidOperationException("Detected parameter token but failed to read");
        }

        token = null;
        return false;
    }

    public BicepEnumToken(string name, string[] tokens) : base(name, "string", $"{name}Options")
    {
        Tokens = tokens;
    }
}