// See https://aka.ms/new-console-template for more information

using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

var bicepPath = args[0];
var bicepOutputPath = args[1];

foreach (var file in Directory.GetFiles(bicepPath, "*.bicep", SearchOption.AllDirectories))
    await GenerateBicepWrapper(Path.GetFileName(file), bicepOutputPath, await File.ReadAllLinesAsync(file));

static Task GenerateBicepWrapper(string fileName, string outputPath, string[] contents)
{
    var inputs = GetParameters(contents).ToArray();
    var outputs = GetOutputs(contents).ToArray();
    GenerateAssembly(fileName, outputPath,  SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, contents))), inputs, outputs);
    return Task.CompletedTask;
}

static void GenerateAssembly(string fileName, string outputPath, byte[] contentsHash, BicepParameter[] inputs, BicepOutput[] outputs)
{
    var classTemplate = @$"public class BicepTemplate {{}}

public class {PascalCase(Path.GetFileNameWithoutExtension(fileName))} : BicepTemplate {{
    private string _fileName = ""{fileName}"";
    private string _fileHash = ""{Convert.ToBase64String(contentsHash)}"";

{string.Join(Environment.NewLine, inputs.Select(x => @$"
private {x.BicepType} {x.Name};
public {x.BicepType} {PascalCase(x.Name)} {{ get {{ return this.{x.Name}; }} set {{ this.{x.Name} = value; }} }}"))}

}}
";
    
    var tree = SyntaxFactory.ParseSyntaxTree(classTemplate);
    var systemRefLocation=typeof(object).GetTypeInfo().Assembly.Location;
    var systemReference = MetadataReference.CreateFromFile(systemRefLocation);

    var compilation = CSharpCompilation.Create(fileName)
        .WithOptions(
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
        .AddReferences(systemReference)
        .AddSyntaxTrees(tree);
    
    var path = Path.Combine(outputPath, $"{fileName}.dll");
    var compilationResult = compilation.Emit(path);    
}

static string PascalCase(string fileName)
{
    var info = CultureInfo.CurrentCulture.TextInfo;
    return info.ToTitleCase(fileName)
        .Replace("-", "")
        .Replace(" ", "");
}

static IEnumerable<BicepParameter> GetParameters(string[] contents)
{
    var bicepParameterRegex = new Regex(@"^\s*param\s+(.*?)\s+(.*?)\s*$");
    foreach (var line in contents)
    {
        var match = bicepParameterRegex.Match(line);
        if (match.Success)
            yield return new BicepParameter(
                match.Groups[1].Value,
                match.Groups[2].Value
            );
    }
}

static IEnumerable<BicepOutput> GetOutputs(string[] contents)
{
    var bicepParameterRegex = new Regex(@"^\s*output\s+(.*?)\s+(.*?)\s*\=.*?$");
    foreach (var line in contents)
    {
        var match = bicepParameterRegex.Match(line);
        if (match.Success)
            yield return new BicepOutput(
                match.Groups[1].Value,
                match.Groups[2].Value
            );
    }
}

internal class BicepParameter
{
    public BicepParameter(string name, string bicepType)
    {
        Name = name;
        BicepType = bicepType;
    }

    public string Name { get; set; }
    public string BicepType { get; set; }
}

internal class BicepOutput
{
    public BicepOutput(string name, string bicepType)
    {
        Name = name;
        BicepType = bicepType;
    }

    public string Name { get; set; }
    public string BicepType { get; set; }
}
