using System.Text.RegularExpressions;

namespace BicepFlex;

public class BicepReferenceExpression : BicepExpression
{
    private static readonly Regex Regex = new Regex(@"^\s*([A-Za-z0-9\-]*)\s*$");

    private BicepReferenceExpression(string reference)
    {
        Reference = reference;
    }

    public string Reference { get; }

    public static bool TryParse(string expression, out BicepReferenceExpression? expr)
    {
        var match = Regex.Match(expression);
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
    public override bool InferType(IEnumerable<BicepToken> tokens)
    {
        if (InferredType != null && InferredType != "object")
        {
            return false;
        }

        var parameter = tokens.OfType<BicepParameterToken>().SingleOrDefault(x => x.Name == Reference);
        if (parameter != null)
        {
            InferredType = parameter.CustomType ?? parameter.BicepType;
            return true;
        }
        var variable = tokens.OfType<BicepVariableToken>().SingleOrDefault(x => x.Name == Reference);
        if (variable != null)
        {
            InferredType = variable.InferredType;
            return true;
        }

        return false;
    }
}