using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Resources.Models;
using BicepRunner;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                await subscription.GetResourceGroups().CreateOrUpdateAsync(_resourceGroup, new ResourceGroupData(Location.AustraliaEast));
                rg = await subscription.GetResourceGroups().GetIfExistsAsync(_resourceGroup);
            }

            var deploymentTask = await rg.Value.GetDeployments().CreateOrUpdateAsync(
                "bicep-flex-test",
                new DeploymentInput(new DeploymentProperties(DeploymentMode.Incremental)
                {
                    Template = JsonDocument.Parse(json).RootElement,
                    Parameters =  JsonDocument.Parse(JsonConvert.SerializeObject(buildParameters)).RootElement
                }));

            return template.BuildOutput((Dictionary<string, object>)deploymentTask.Value.Data.Properties.Outputs);
        }
        finally
        {
            File.Delete(temp);
        }
    }
}