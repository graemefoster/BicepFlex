using System.Text.RegularExpressions;

namespace BicepFlex;

public class BicepVarVariableReferenceToken : BicepToken
{
    private static readonly Regex regex = new(@"^\s*var\s+([A-Za-z0-9_]*)\s*\=\s*([A-Za-z0-9_]*)\s*$");

    public BicepVarVariableReferenceToken(string name, string references)
    {
        Name = name;
        References = references;
    }

    public string References { get; }

    public static bool TryParse(IEnumerator<string> reader, out BicepVarVariableReferenceToken? token)
    {
        var line = reader.Current;
        var match = regex.Match(line);
        if (match.Success)
        {
            token = new BicepVarVariableReferenceToken(match.Groups[1].Value, match.Groups[2].Value);
            return true;
        }

        token = null;
        return false;
    }

    public bool InferType(IEnumerable<BicepToken> otherTokens)
    {
        var reference = otherTokens.SingleOrDefault(x => x.Name == References);
        if (reference != null)
        {
            if (reference is BicepParameterToken parameterToken)
            {
                if (parameterToken.CustomType != null)
                {
                    InferredType = parameterToken.CustomType;
                    Console.WriteLine($"Inferred variable {base.Name} to be type {InferredType}");
                    return true;
                }
            }
            if (reference is BicepVarVariableReferenceToken varToken)
            {
                if (varToken.InferredType != null)
                {
                    InferredType = varToken.InferredType;
                    Console.WriteLine($"Inferred variable {base.Name} to be type {InferredType}");
                    return true;
                }
            }
        }
        return false;
    }

    public string? InferredType { get; set; }
}