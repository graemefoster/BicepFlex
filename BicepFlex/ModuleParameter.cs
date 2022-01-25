namespace BicepFlex;

public class ModuleParameter
{
    public string Name { get; }
    public string VariableOrParameterReference { get; }
    public string? InferredType { get; private set; }

    public ModuleParameter(string name, string variableOrParameterReference)
    {
        Name = name;
        VariableOrParameterReference = variableOrParameterReference;
    }


    public bool InferType(IEnumerable<BicepToken> tokens)
    {
        if (InferredType != null && InferredType  != "object") return false;

        var newInferredType = InferredType;
        var parameter = tokens.OfType<BicepParameterToken>().SingleOrDefault(x => x.Name == VariableOrParameterReference);
        if (parameter != null)
        {
            newInferredType = parameter.CustomType ?? parameter.BicepType;
        }
        var variable = tokens.OfType<BicepVarVariableReferenceToken>().SingleOrDefault(x => x.Name == VariableOrParameterReference);
        if (variable != null)
        {
            newInferredType = variable.InferredType;
        }

        var foundSpecificType = newInferredType != InferredType;
        InferredType = newInferredType;
        return foundSpecificType;
    }
}