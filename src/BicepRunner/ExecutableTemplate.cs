namespace BicepRunner;

public static class ExecutableTemplate
{
    public static ExecutableTemplate<TIn> SubscriptionScope<TIn>(BicepSubscriptionScopeTemplate<TIn> template)
        where TIn : BicepOutput
    {
        return new ExecutableTemplate<TIn>(template);
    }

    public static ExecutableTemplate<TIn> ResourceGroupScope<TIn>(string resourceGroup,
        BicepResourceGroupScopeTemplate<TIn> template) where TIn : BicepOutput
    {
        return new ExecutableTemplate<TIn>(resourceGroup, template);
    }
}

public class ExecutableTemplate<T> where T : BicepOutput
{
    private readonly string? _resourceGroup;
    private readonly BicepTemplate<T> _template;

    internal ExecutableTemplate(BicepSubscriptionScopeTemplate<T> template)
    {
        _template = template;
    }

    internal ExecutableTemplate(string resourceGroup, BicepResourceGroupScopeTemplate<T> template)
    {
        _resourceGroup = resourceGroup;
        _template = template;
    }

    public Dictionary<string, object> BuildParameters() => _template.BuildParameters();
    public string FileName => _template.FileName;

    public bool IsResourceGroup(out string? resourceGroup)
    {
        resourceGroup = _resourceGroup;
        return resourceGroup != null;
    }

    public T BuildOutput(Dictionary<string, object> propertiesOutputs)
    {
        return _template.BuildOutput(propertiesOutputs);
    }
}