using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Resources.Models;
using BicepRunner;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace AzBicepRunner;

public class AzBicepRunner : IBicepRunner
{
    private readonly ArmClient _armClient;
    private readonly string _bicepRoot;

    public AzBicepRunner(string bicepRoot)
    {
        _bicepRoot = Path.GetFullPath(bicepRoot)!;
        _armClient = new ArmClient(new DefaultAzureCredential());
    }

    public async Task<INextStep<TState>> ExecuteTemplate<T, TState>(ExecutableTemplate<T> template,
        Func<T, TState> stateGenerator) where T : BicepOutput
    {
        return new AzNextStepRunner<TState>(stateGenerator(await ExecuteTemplateInternal(template)), this);
    }

    public async Task<INextStep<T>> ExecuteTemplate<T>(ExecutableTemplate<T> template) where T : BicepOutput
    {
        return new AzNextStepRunner<T>(await ExecuteTemplateInternal(template), this);
    }

    public async Task<INextStep<Tuple<TState1, TState2, TState3>>> ExecuteTemplate<T1, T2, T3, TState1, TState2,
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
        where T3 : BicepOutput
    {
        var task1 = ExecuteTemplateInternal(template1);
        var task2 = ExecuteTemplateInternal(template2);
        var task3 = ExecuteTemplateInternal(template3);
        await Task.WhenAll(task1, task2, task3);

        return new AzNextStepRunner<Tuple<TState1, TState2, TState3>>(
            new Tuple<TState1, TState2, TState3>(state1Generator(await task1), state2Generator(await task2),
                state3Generator(await task3)),
            this
        );
    }

    public async Task<INextStep<Tuple<TState1, TState2>>> ExecuteTemplate<T1, T2, TState1, TState2>(
        ExecutableTemplate<T1> template1,
        Func<T1, TState1> state1Generator,
        ExecutableTemplate<T2> template2,
        Func<T2, TState2> state2Generator)
        where T1 : BicepOutput
        where T2 : BicepOutput
    {
        var task1 = ExecuteTemplateInternal(template1);
        var task2 = ExecuteTemplateInternal(template2);
        await Task.WhenAll(task1, task2);
        return new AzNextStepRunner<Tuple<TState1, TState2>>(
            new Tuple<TState1, TState2>(state1Generator(await task1), state2Generator(await task2)),
            this
        );
    }

    private async Task<T> ExecuteTemplateInternal<T>(ExecutableTemplate<T> template, string? deploymentName = null)
        where T : BicepOutput
    {
        var buildParameters = template.BuildParameters();

        var temp = Path.GetTempFileName();
        try
        {
            var bicepFile = Path.Combine(_bicepRoot, template.FileName);
            var bicepToJsonProcess =
                new ProcessStartInfo("az", $"bicep build --file {bicepFile} --outfile \"{temp}\"")
                {
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
            var p = Process.Start(bicepToJsonProcess)!;
            await p.WaitForExitAsync();

            if (p.ExitCode != 0) throw new InvalidOperationException("Failed to execute az");

            var json = await File.ReadAllTextAsync(temp, Encoding.UTF8);

            var subscription = await _armClient.GetDefaultSubscriptionAsync();
            if (template.IsResourceGroup(out var _resourceGroup))
            {
                var rg = await subscription.GetResourceGroups().GetIfExistsAsync(_resourceGroup);
                if (rg.Value == null)
                {
                    await subscription.GetResourceGroups()
                        .CreateOrUpdateAsync(_resourceGroup, new ResourceGroupData(Location.AustraliaEast));
                    rg = await subscription.GetResourceGroups().GetIfExistsAsync(_resourceGroup);
                }

                var deploymentTask = await rg.Value.GetDeployments().CreateOrUpdateAsync(
                    deploymentName ?? $"bicep-flex-{DateTimeOffset.Now.ToUnixTimeSeconds()}",
                    new DeploymentInput(new DeploymentProperties(DeploymentMode.Incremental)
                    {
                        Template = JsonDocument.Parse(json).RootElement,
                        Parameters = JsonDocument.Parse(JsonConvert.SerializeObject(buildParameters,
                            new JsonSerializerSettings
                            {
                                Converters = new[] { new StringEnumConverter() }
                            })).RootElement
                    }));

                return template.BuildOutput((Dictionary<string, object>)deploymentTask.Value.Data.Properties.Outputs);
            }

            var subscriptionDeploymentTask = await subscription.GetDeployments().CreateOrUpdateAsync(
                deploymentName ?? $"bicep-flex-{DateTimeOffset.Now.ToUnixTimeSeconds()}",
                new DeploymentInput(new DeploymentProperties(DeploymentMode.Incremental)
                {
                    Template = JsonDocument.Parse(json).RootElement,
                    Parameters = JsonDocument.Parse(JsonConvert.SerializeObject(buildParameters,
                        new JsonSerializerSettings
                        {
                            Converters = new[] { new StringEnumConverter() }
                        })).RootElement
                }));

            return template.BuildOutput(
                (Dictionary<string, object>)subscriptionDeploymentTask.Value.Data.Properties.Outputs);
        }
        finally

        {
            File.Delete(temp);
        }
    }
}