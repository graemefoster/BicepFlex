using System.Diagnostics;
using System.Text;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Resources.Models;
using BicepRunner;

namespace AzBicepRunner;

public class AzBicepRunner : IBicepRunner
{
    private readonly AzureLocation _location;
    private readonly ArmClient _armClient;
    private readonly string _bicepRoot;
    private readonly string _subscriptionId;
    private SubscriptionResource? _subscription;

    public AzBicepRunner(string bicepRoot, AzureLocation location, Guid subscriptionId)
    {
        _location = location;
        _subscriptionId = subscriptionId.ToString().ToLowerInvariant();
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
        _subscription ??= (await _armClient.GetSubscriptions().GetAsync(_subscriptionId)).Value;

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

            ArmOperation<ArmDeploymentResource> deploymentTask;
            var armDeploymentContent = new ArmDeploymentContent(
                new ArmDeploymentProperties(ArmDeploymentMode.Incremental)
                {
                    Template = BinaryData.FromString(json),
                    Parameters = BinaryData.FromObjectAsJson(buildParameters)
                });

            if (template.IsResourceGroup(out var resourceGroup))
            {
                ResourceGroupResource? rg = null;
                if (await _subscription.GetResourceGroups().ExistsAsync(resourceGroup))
                {
                    rg = (await _subscription.GetResourceGroups().CreateOrUpdateAsync(WaitUntil.Completed,
                        resourceGroup,
                        new ResourceGroupData(_location))).Value;
                }
                else
                {
                    rg = await _subscription.GetResourceGroups().GetAsync(resourceGroup);
                }

                deploymentTask = await rg.GetArmDeployments().CreateOrUpdateAsync(
                    WaitUntil.Completed,
                    deploymentName ?? $"bicep-flex-{DateTimeOffset.Now.ToUnixTimeSeconds()}",
                    armDeploymentContent);
            }
            else
            {
                armDeploymentContent.Location = _location;
                deploymentTask = await _subscription.GetArmDeployments().CreateOrUpdateAsync(
                    WaitUntil.Completed,
                    deploymentName ?? $"bicep-flex-{DateTimeOffset.Now.ToUnixTimeSeconds()}",
                    armDeploymentContent);
            }

            var output = deploymentTask.WaitForCompletionResponseAsync();
            return template.BuildOutput(output.Result.Content.ToObjectFromJson<Dictionary<string, object>>());
        }
        finally

        {
            File.Delete(temp);
        }
    }
}