namespace BicepRunner;

public static class RunnerEx
{
    public static async Task<INextStep<Tuple<T1NextState, T2NextState, T3NextState>>> ThenDeploy<TState, T1, T2, T3,
        T1NextState, T2NextState, T3NextState>(
        this Task<INextStep<TState>> obj,
        Func<TState, ExecutableTemplate<T1>> template1,
        Func<T1, T1NextState> state1Generator,
        Func<TState, ExecutableTemplate<T2>> template2,
        Func<T2, T2NextState> state2Generator,
        Func<TState, ExecutableTemplate<T3>> template3,
        Func<T3, T3NextState> state3Generator
    )
        where T1 : BicepOutput
        where T2 : BicepOutput
        where T3 : BicepOutput
    {
        return await (await obj).ThenDeploy(template1, state1Generator, template2, state2Generator, template3,
            state3Generator);
    }

    public static async Task<INextStep<Tuple<T1NextState, T2NextState>>> ThenDeploy<TState, T1, T2, T1NextState,
        T2NextState>(
        this Task<INextStep<TState>> obj,
        Func<TState, ExecutableTemplate<T1>> template1,
        Func<T1, T1NextState> state1Generator,
        Func<TState, ExecutableTemplate<T2>> template2,
        Func<T2, T2NextState> state2Generator
    )
        where T1 : BicepOutput
        where T2 : BicepOutput
    {
        return await (await obj).ThenDeploy(template1, state1Generator, template2, state2Generator);
    }


    public static async Task<INextStep<TNextState>> ThenDeploy<T1, TState, TNextState>(
        this Task<INextStep<TState>> obj,
        Func<TState, ExecutableTemplate<T1>> template,
        Func<T1, TNextState> stateGenerator) where T1 : BicepOutput
    {
        return await (await obj).ThenDeploy(template, stateGenerator);
    }

    public static async Task<INextStep<T1>> ThenDeploy<T1, TState>(
        this Task<INextStep<TState>> obj,
        Func<TState, ExecutableTemplate<T1>> template) where T1 : BicepOutput
    {
        return await (await obj).ThenDeploy(template);
    }
}