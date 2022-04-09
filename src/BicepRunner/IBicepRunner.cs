namespace BicepRunner;

public interface IBicepRunner
{
    Task<INextStep<TState>> ExecuteTemplate<T, TState>(ExecutableTemplate<T> template, Func<T, TState> stateGenerator)
        where T : BicepOutput;

    Task<INextStep<T>> ExecuteTemplate<T>(ExecutableTemplate<T> template) where T : BicepOutput;

    Task<INextStep<Tuple<TState1, TState2>>> ExecuteTemplate<T1, T2, TState1, TState2>(
        ExecutableTemplate<T1> template1,
        Func<T1, TState1> state1Generator,
        ExecutableTemplate<T2> template2,
        Func<T2, TState2> state2Generator)
        where T1 : BicepOutput
        where T2 : BicepOutput;

    Task<INextStep<Tuple<TState1, TState2, TState3>>> ExecuteTemplate<T1, T2, T3, TState1, TState2,
        TState3>(
        ExecutableTemplate<T1> template1,
        Func<T1, TState1> state1Generator,
        ExecutableTemplate<T2> template2,
        Func<T2, TState2> state2Generator,
        ExecutableTemplate<T3> template3,
        Func<T3, TState3> state3Generator
    )
        where T1 : BicepOutput
        where T2 : BicepOutput
        where T3 : BicepOutput;
}