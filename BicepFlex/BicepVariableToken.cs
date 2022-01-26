using System.Reflection;
using System.Text.RegularExpressions;

namespace BicepFlex;

public class BicepVariableToken : BicepToken
{
    private static readonly Regex regex = new(@"^\s*var\s+([A-Za-z0-9_]*)\s*\=\s*([A-Za-z0-9_]*)\s*$");
    public BicepExpression Expression { get; }

    public BicepVariableToken(string name, string expressionText)
    {
        Name = name;
        if (BicepExpression.Parse(expressionText, out var expr)) Expression = expr!;
    }

    public string References { get; }

    public static bool TryParse(IEnumerator<string> reader, out BicepVariableToken? token)
    {
        var line = reader.Current;
        var match = regex.Match(line);
        if (match.Success)
        {
            token = new BicepVariableToken(match.Groups[1].Value, match.Groups[2].Value);
            return true;
        }

        token = null;
        return false;
    }

    public bool InferType(IEnumerable<BicepToken> tokens, Assembly referenceTypeAssembly)
    {
        return Expression?.InferType(tokens, referenceTypeAssembly) ?? false;
    }

    public string? InferredType => Expression?.InferredType;
}