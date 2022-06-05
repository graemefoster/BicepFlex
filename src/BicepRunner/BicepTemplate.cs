namespace BicepRunner;

public abstract class BicepTemplate<T>: IBicepTemplate where T : BicepOutput
{
    public abstract string FileName { get; }
    public abstract string FileHash { get; }
    public abstract (Type, string)[] References { get; }
    public abstract Type[] ReferencedBy { get; }
    public abstract Dictionary<string, object> BuildParameters();
    public abstract T BuildOutput(Dictionary<string, object> outputs);
}