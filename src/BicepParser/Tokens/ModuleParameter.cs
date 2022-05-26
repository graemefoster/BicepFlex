using System.Reflection;
using BicepFlex.Expressions;

namespace BicepFlex.Tokens;

public class ModuleParameter
{
    public string Name { get; }
    public BicepExpression? Expression { get; }
    public string? InferredType => Expression?.InferredType;

    public ModuleParameter(string name, string expressionText)
    {
        Name = name;
        if (BicepExpression.Parse(expressionText, out var expr))
        {
            Expression = expr!;
        }
    }

    public bool InferType(IEnumerable<BicepToken> tokens, Assembly? referenceTypeAssembly)
    {
        return Expression?.InferType(tokens, referenceTypeAssembly) ?? false;
    }
}