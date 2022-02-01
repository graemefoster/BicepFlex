using System.Reflection;
using BicepFlex.Tokens;

namespace BicepFlex.Expressions;

public abstract class BicepExpression
{
    public string? InferredType { get; protected set; }
    public static bool Parse(string expressionString, out BicepExpression? expression)
    {
        if (BicepObjectTraversalReferenceExpression.TryParse(expressionString, out var expr1))
        {
            expression = expr1;
            return true;
        }
        if (BicepReferenceExpression.TryParse(expressionString, out var expr))
        {
            expression = expr;
            return true;
        }

        expression = null;
        return false;
    }

    public abstract bool InferType(IEnumerable<BicepToken> tokens, Assembly referenceTypeAssembly);
}