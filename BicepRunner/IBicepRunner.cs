namespace BicepRunner;

public interface IBicepRunner
{
    Task<T> ExecuteTemplate<T>(BicepTemplate<T> template) where T : BicepOutput;
}