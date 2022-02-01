namespace BicepRunner;

public abstract class BicepTemplate<T> where T : BicepOutput
{
    public virtual string FileName { get; } = default!;
    public virtual string FileHash { get; } = default!;

    public abstract Dictionary<string, object> BuildParameters();
    public abstract T BuildOutput(Dictionary<string, object> outputs);
}