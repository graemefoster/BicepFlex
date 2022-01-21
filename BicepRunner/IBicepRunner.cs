namespace BicepRunner;

public interface IBicepRunner
{
    Task<INextStep<TState>> ExecuteTemplate<T, TState>(BicepTemplate<T> template, Func<T, TState> stateGenerator)
        where T : BicepOutput;
}