using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Resources.Models;
using BicepRunner;
using Newtonsoft.Json;

namespace AzBicepRunner;

public class AzBicepRunner : IBicepRunner
{
    private readonly ArmClient _armClient;
    private readonly string _resourceGroup;
    private readonly string _bicepRoot;

    public AzBicepRunner(string resourceGroup, string bicepRoot)
    {
        _resourceGroup = resourceGroup;
        _bicepRoot = bicepRoot;
        _armClient = new ArmClient(new DefaultAzureCredential());
    }

    public async Task<INextStep<TState>> ExecuteTemplate<T, TState>(BicepTemplate<T> template,
        Func<T, TState> stateGenerator) where T : BicepOutput
    {
        return new AzNextStepRunner<TState>(stateGenerator(await ExecuteTemplate(template)), this);
    }

    public async Task<INextStep<Tuple<TState1, TState2, TState3>>> ExecuteTemplate<T1, T2, T3, TState1, TState2,
        TState3>(
        BicepTemplate<T1> template1,
        Func<T1, TState1> state1Generator,
        BicepTemplate<T2> template2,
        Func<T2, TState2> state2Generator,
        BicepTemplate<T3> template3,
        Func<T3, TState3> state3Generator
    )
        where T1 : BicepOutput
        where T2 : BicepOutput
        where T3 : BicepOutput
    {
        var task1 = ExecuteTemplate(template1);
        var task2 = ExecuteTemplate(template2);
        var task3 = ExecuteTemplate(template3);
        await Task.WhenAll(task1, task2, task3);

        return new AzNextStepRunner<Tuple<TState1, TState2, TState3>>(
            new Tuple<TState1, TState2, TState3>(state1Generator(await task1), state2Generator(await task2),
                state3Generator(await task3)),
            this
        );
    }

    public async Task<INextStep<Tuple<TState1, TState2>>> ExecuteTemplate<T1, T2, TState1, TState2>(
        BicepTemplate<T1> template1,
        Func<T1, TState1> state1Generator,
        BicepTemplate<T2> template2,
        Func<T2, TState2> state2Generator)
        where T1 : BicepOutput
        where T2 : BicepOutput
    {
        var task1 = ExecuteTemplate(template1);
        var task2 = ExecuteTemplate(template2);
        await Task.WhenAll(task1, task2);
        return new AzNextStepRunner<Tuple<TState1, TState2>>(
            new Tuple<TState1, TState2>(state1Generator(await task1), state2Generator(await task2)),
            this
        );
    }

    public async Task<T> ExecuteTemplate<T>(BicepTemplate<T> template) where T : BicepOutput
    {
        var buildParameters = template.BuildParameters();

        var temp = Path.GetTempFileName();
        try
        {
            var bicepFile = Path.Combine(_bicepRoot, template.FileName);
            var bicepToJsonProcess =
                new ProcessStartInfo(@"""c:\Program Files (x86)\Microsoft SDKs\Azure\CLI2\wbin\az.cmd""",
                    $"bicep build --file {bicepFile} --outfile \"{temp}\"")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
            var p = Process.Start(bicepToJsonProcess)!;
            p.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
            p.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);

            await p.WaitForExitAsync();
            if (p.ExitCode != 0)
            {
                throw new InvalidOperationException("Failed to execute az");
            }

            var json = await File.ReadAllTextAsync(temp, Encoding.UTF8);

            var subscription = await _armClient.GetDefaultSubscriptionAsync();
            var rg = await subscription.GetResourceGroups().GetIfExistsAsync(_resourceGroup);
            if (rg.Value == null)
            {
                await subscription.GetResourceGroups()
                    .CreateOrUpdateAsync(_resourceGroup, new ResourceGroupData(Location.AustraliaEast));
                rg = await subscription.GetResourceGroups().GetIfExistsAsync(_resourceGroup);
            }

            var deploymentTask = await rg.Value.GetDeployments().CreateOrUpdateAsync(
                "bicep-flex-test",
                new DeploymentInput(new DeploymentProperties(DeploymentMode.Incremental)
                {
                    Template = JsonDocument.Parse(json).RootElement,
                    Parameters = JsonDocument.Parse(JsonConvert.SerializeObject(buildParameters)).RootElement
                }));

            return template.BuildOutput((Dictionary<string, object>)deploymentTask.Value.Data.Properties.Outputs);
        }
        finally

        {
            File.Delete(temp);
        }
    }
}