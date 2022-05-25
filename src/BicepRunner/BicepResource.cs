namespace BicepRunner;

/// <summary>
/// Wrappers around resource Ids
/// </summary>
public class BicepResource
{
    public string ResourceId { get; }

    public BicepResource(string resourceId)
    {
        ResourceId = resourceId;
    } 
}