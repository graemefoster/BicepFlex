using Azure.ResourceManager.Resources.Models;
using BicepRunner;

namespace AzBicepRunner;

public class AzNextStepRunner<TState> : INextStep<TState>
{
    private readonly IBicepRunner _runner;

    public AzNextStepRunner(TState output, IBicepRunner runner)
    {
        Output = output;
        _runner = runner;
    }

    public Task<INextStep<Tuple<TState1, TState2, TState3>>> ThenDeploy<T1, T2, T3, TState1, TState2, TState3>(
        Func<TState, BicepTemplate<T1>> template1, Func<T1, TState1> state1Generator,
        Func<TState, BicepTemplate<T2>> template2,
        Func<T2, TState2> state2Generator, Func<TState, BicepTemplate<T3>> template3, Func<T3, TState3> state3Generator)
        where T1 : BicepOutput where T2 : BicepOutput where T3 : BicepOutput
    {
        return _runner.ExecuteTemplate(template1(Output), state1Generator, template2(Output), state2Generator,
            template3(Output), state3Generator);
    }

    public TState Output { get; }

    public async Task<T1> ThenDeploy<T1>(Func<TState, BicepTemplate<T1>> template) where T1 : BicepOutput
    {
        return (await _runner.ExecuteTemplate(template(Output), s => s)).Output;
    }

    public Task<INextStep<TNextState>> ThenDeploy<T1, TNextState>(Func<TState, BicepTemplate<T1>> template,
        Func<T1, TNextState> stateGenerator) where T1 : BicepOutput
    {
        return _runner.ExecuteTemplate(template(Output), stateGenerator);
    }

    public Task<INextStep<Tuple<TState1, TState2>>> ThenDeploy<T1, T2, TState1, TState2>(
        Func<TState, BicepTemplate<T1>> template1, Func<T1, TState1> state1Generator,
        Func<TState, BicepTemplate<T2>> template2, Func<T2, TState2> state2Generator)
        where T1 : BicepOutput where T2 : BicepOutput
    {
        return _runner.ExecuteTemplate(template1(Output), state1Generator, template2(Output), state2Generator);
    }
}