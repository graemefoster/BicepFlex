// See https://aka.ms/new-console-template for more information

using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using BicepFlex;
using BicepRunner;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

var bicepPath = args[0];
var bicepOutputPath = args[1];

foreach (var file in Directory.GetFiles(bicepPath, "*.bicep", SearchOption.AllDirectories))
    await GenerateBicepWrapper(Path.GetFileName(file), bicepOutputPath, await File.ReadAllLinesAsync(file));

static Task GenerateBicepWrapper(string fileName, string outputPath, string[] contents)
{
    var meta = new BicepFileParser().Parse(contents);
    var inputs = meta.Parameters;
    var outputs = meta.Outputs;
    GenerateAssembly(fileName, outputPath,
        SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, contents))), inputs,
        outputs);
    return Task.CompletedTask;
}

static void GenerateAssembly(string fileName, string outputPath, byte[] contentsHash, 
    IEnumerable<BicepParameterToken> inputs,
    IEnumerable<BicepOutputToken> outputs)
{
    var pascalCaseName = PascalCase(Path.GetFileNameWithoutExtension(fileName));
    var classTemplate = @$"using BicepRunner;
using System.Reflection;
using System.Collections.Generic;

public class {pascalCaseName} : BicepTemplate<{pascalCaseName}.{pascalCaseName}Output> {{
    public override string FileName => ""{fileName}"";
    public override string FileHash => ""{Convert.ToBase64String(contentsHash)}"";

{string.Join(Environment.NewLine, inputs.Select(x => @$"
private {x.DotNetTypeName()} _{x.Name};
public {x.DotNetTypeName()} {PascalCase(x.Name)} {{ get {{ return this._{x.Name}; }} set {{ this._{x.Name} = value; }} }}"))}

    public class {pascalCaseName}Output : BicepOutput {{
        {string.Join(Environment.NewLine, outputs.Select(x => @$"

        private {x.DotNetTypeName()} _{x.Name};
        public {x.DotNetTypeName()} {PascalCase(x.Name)} {{ get {{ return this._{x.Name}; }} set {{ this._{x.Name} = value; }} }}"))}

        public {pascalCaseName}Output(Dictionary<string, object> outputs) {{
            base.SetProperties(outputs);
        }}
    }}

    public override Dictionary<string, object> BuildParameters() {{
        var dictionary = new Dictionary<string, object>();
        {string.Join(Environment.NewLine, inputs.Select(x => @$"dictionary[""{x.Name}""] = new {{ value = this._{x.Name}}};"))}
        return dictionary;
    }} 

    public override {pascalCaseName}Output BuildOutput(Dictionary<string, object> outputs) {{
        return new {pascalCaseName}Output(outputs);
    }} 
}}
";

    var tree = SyntaxFactory.ParseSyntaxTree(classTemplate);

    var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
    var references = new List<MetadataReference>();

    references.Add(MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location));
    references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "netstandard.dll")));
    references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Collections.dll")));
    references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")));
    references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Private.CoreLib.dll")));

    var bicepRunner = typeof(IBicepRunner).GetTypeInfo().Assembly.Location;
    var bicepRunnerReference = MetadataReference.CreateFromFile(bicepRunner);
    references.Add(bicepRunnerReference);

    var compilation = CSharpCompilation.Create(fileName)
        .WithOptions(
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable,
                assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default))
        .AddReferences(references)
        .AddSyntaxTrees(tree);

    var path = Path.Combine(outputPath, $"{fileName}.dll");
    var compilationResult = compilation.Emit(path);
    if (!compilationResult.Success)
    {
        foreach (var issue in compilationResult.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error))
            Console.WriteLine(issue.ToString());
        throw new InvalidOperationException($"Failed to generate compiled library: {compilationResult.Diagnostics}");
    }
}

static string PascalCase(string fileName)
{
    var info = CultureInfo.CurrentCulture.TextInfo;
    return info.ToTitleCase(fileName)
        .Replace("-", "")
        .Replace(" ", "");
}