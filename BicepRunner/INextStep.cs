namespace BicepRunner;

public interface INextStep<TState>
{
    Task<INextStep<TNextState>> ThenDeploy<T1, TNextState>(Func<TState, BicepTemplate<T1>> template,
        Func<T1, TNextState> stateGenerator) where T1 : BicepOutput;

    TState Output { get; }
}