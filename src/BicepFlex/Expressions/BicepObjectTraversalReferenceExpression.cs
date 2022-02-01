using System.Reflection;
using System.Text.RegularExpressions;
using BicepFlex.Tokens;

namespace BicepFlex.Expressions;

public class BicepObjectTraversalReferenceExpression : BicepReferenceExpression
{
    private static readonly Regex Regex = new(@"^\s*([A-Za-z0-9\-]+)\.([A-Za-z0-9\-\.]+)\s*$");

    private BicepObjectTraversalReferenceExpression(string reference, string traversals): base(reference)
    {
        Traversals = traversals.Split('.');
    }

    public string[] Traversals { get; }

    public static bool TryParse(string expression, out BicepObjectTraversalReferenceExpression? expr)
    {
        var match = Regex.Match(expression);
        if (match.Success)
        {
            expr = new BicepObjectTraversalReferenceExpression(
                match.Groups[1].Value,
                match.Groups[2].Value);
            return true;
        }

        expr = null;
        return false;
    }

    /// <summary>
    /// Look for a variable or a parameter of which we know the type:
    /// </summary>
    /// <param name="tokens"></param>
    /// <param name="referenceTypeAssembly"></param>
    /// <returns></returns>
    public override bool InferType(IEnumerable<BicepToken> tokens, Assembly? referenceTypeAssembly)
    {
        if (base.InferType(tokens, referenceTypeAssembly))
        {
            //got a type - now walk the tree
            var type = referenceTypeAssembly!.GetType(InferredType!);
            if (type == null)
            {
                Console.WriteLine($"WARNING:: Could not find type {InferredType} in Reference Type assembly:{referenceTypeAssembly.GetName().Name}");
                return false;
            }

            var good = true;
            foreach (var part in Traversals)
            {
                var prop = type.GetProperty(part, BindingFlags.Public | BindingFlags.Instance);
                if (prop != null)
                {
                    type = prop.PropertyType;
                    InferredType = type.FullName;
                }
                else
                {
                    good = false;
                    Console.WriteLine($"Failed to find property {part} on type {type.FullName}");
                    break;
                }
            }

            if (!good)
            {
                InferredType = null;
                return false;
            }
            
            return true;
        }

        return false;
    }
}