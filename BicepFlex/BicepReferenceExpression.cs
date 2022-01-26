using System.Text.RegularExpressions;

namespace BicepFlex;

public class BicepReferenceExpression : BicepExpression
{
    private static Regex regex = new Regex(@"^\s*([A-Za-z0-9\-]*)\s*$");

    private BicepReferenceExpression(string reference)
    {
        Reference = reference;
    }

    public string Reference { get; }

    public static bool TryParse(string expression, out BicepReferenceExpression? expr)
    {
        var match = regex.Match(expression);
        if (match.Success)
        {
            expr = new BicepReferenceExpression(match.Groups[1].Value);
            return true;
        }

        expr = null;
        return false;
    }

    /// <summary>
    /// Look for a variable or a parameter of which we know the type:
    /// </summary>
    /// <param name="tokens"></param>
    /// <param name="inferredType"></param>
    /// <returns></returns>
    public override bool InferType(IEnumerable<BicepToken> tokens, out string? inferredType)
    {
        var parameter = tokens.OfType<BicepParameterToken>().SingleOrDefault(x => x.Name == Reference);
        if (parameter != null)
        {
            inferredType = parameter.CustomType ?? parameter.BicepType;
            return true;
        }
        var variable = tokens.OfType<BicepVariableToken>().SingleOrDefault(x => x.Name == Reference);
        if (variable != null)
        {
            inferredType = variable.InferredType;
            return true;
        }

        inferredType = null;
        return false;
    }
}