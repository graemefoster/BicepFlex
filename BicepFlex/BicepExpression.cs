namespace BicepFlex;

public abstract class BicepExpression
{
    public string? InferredType { get; protected set; }
    public static bool Parse(string expressionString, out BicepExpression? expression)
    {
        if (BicepReferenceExpression.TryParse(expressionString, out var expr))
        {
            expression = expr;
            return true;
        }

        expression = null;
        return false;
    }

    public abstract bool InferType(IEnumerable<BicepToken> tokens);
}