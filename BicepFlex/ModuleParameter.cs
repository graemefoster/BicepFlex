namespace BicepFlex;

public class ModuleParameter
{
    public string Name { get; }
    public BicepExpression? Expression { get; }
    public string? InferredType { get; private set; }

    public ModuleParameter(string name, string expressionText)
    {
        Name = name;
        if (BicepExpression.Parse(expressionText, out var expr))
        {
            Expression = expr!;
        }
    }

    public bool InferType(IEnumerable<BicepToken> tokens)
    {
        if (InferredType != null && InferredType  != "object") return false;
        if (Expression?.InferType(tokens, out var inferredType) ?? false)
        {
            if (InferredType != inferredType)
            {
                InferredType = inferredType;
                return true;
            }
        }
        return false;
    }
}