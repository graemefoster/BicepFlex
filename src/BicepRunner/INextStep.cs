namespace BicepRunner;

public interface INextStep<TState>
{
    TState Output { get; }

    Task<INextStep<T1>> ThenDeploy<T1>(
        Func<TState, ExecutableTemplate<T1>> template)
        where T1 : BicepOutput;

    Task<INextStep<TNextState>> ThenDeploy<T1, TNextState>(
        Func<TState, ExecutableTemplate<T1>> template,
        Func<T1, TNextState> stateGenerator)
        where T1 : BicepOutput;

    Task<INextStep<Tuple<TState1, TState2>>> ThenDeploy<T1, T2, TState1, TState2>(
        Func<TState, ExecutableTemplate<T1>> template1,
        Func<T1, TState1> state1Generator,
        Func<TState, ExecutableTemplate<T2>> template2,
        Func<T2, TState2> state2Generator)
        where T1 : BicepOutput
        where T2 : BicepOutput;

    Task<INextStep<Tuple<TState1, TState2, TState3>>> ThenDeploy<T1, T2, T3, TState1, TState2,
        TState3>(
        Func<TState, ExecutableTemplate<T1>> template1,
        Func<T1, TState1> state1Generator,
        Func<TState, ExecutableTemplate<T2>> template2,
        Func<T2, TState2> state2Generator,
        Func<TState, ExecutableTemplate<T3>> template3,
        Func<T3, TState3> state3Generator
    )
        where T1 : BicepOutput
        where T2 : BicepOutput
        where T3 : BicepOutput;
}