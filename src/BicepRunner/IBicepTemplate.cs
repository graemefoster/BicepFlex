namespace BicepRunner;

public interface IBicepTemplate
{
    public (Type, string)[] References { get; }
    public Type[] ReferencedBy { get; }
    public string FileName { get; }
    public string FileHash { get; }
}