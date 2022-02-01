using System.Globalization;
using System.Reflection;
using BicepFlex.Tokens;
using BicepRunner;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace BicepFlex;

public class BicepFlex
{
    private readonly string _bicepRoot;
    private readonly string _bicepOutputPath;
    private readonly string? _referenceTypesAssembly;

    public BicepFlex(string bicepRoot, string bicepOutputPath, string? referenceTypesAssembly = null)
    {
        _bicepRoot = bicepRoot;
        _bicepOutputPath = bicepOutputPath;
        _referenceTypesAssembly = referenceTypesAssembly;
    }

    public async Task Process()
    {
        var classes = new List<string>();
        var parse = new BicepFileParser();

        var allMetaFiles = await Task.WhenAll(Directory.GetFiles(_bicepRoot, "*.bicep", SearchOption.AllDirectories)
            .Select(async f => parse.Parse(f, await File.ReadAllLinesAsync(f))));

        var referenceTypeAssembly = _referenceTypesAssembly == null ? null : Assembly.LoadFile(_referenceTypesAssembly);
        var postProcess = false;
        while (!postProcess)
        {
            postProcess = false;
            foreach (var file in allMetaFiles)
            {
                if (file.InferTypes(allMetaFiles, referenceTypeAssembly)) postProcess = true;
            }
        }

        foreach (var file in allMetaFiles)
        {
            classes.Add(GenerateBicepClass(file));
        }

        GenerateAssembly(classes.ToArray(), _bicepOutputPath);
    }

    static string GenerateBicepClass(BicepMetaFile file)
    {
        var inputs = file.Parameters;
        var outputs = file.Outputs;

        var pascalCaseName = PascalCase(Path.GetFileNameWithoutExtension(file.ModuleName));
        var classTemplate = @$"
{string.Join(Environment.NewLine, inputs.OfType<BicepEnumToken>().Select(et => $@"
public enum {et.Name}Options {{
    {string.Join(Environment.NewLine, et.Tokens.Select(etv => $"{etv},"))}
}}
"))}

public class {pascalCaseName} : BicepTemplate<{pascalCaseName}.{pascalCaseName}Output> {{
    public override string FileName => ""{file.ModuleName}"";
    public override string FileHash => ""{file.Hash}"";

{string.Join(Environment.NewLine, inputs.Select(x => @$"
private {x.DotNetTypeName()}? _{x.Name};
public {x.DotNetTypeName()}? {PascalCase(x.Name)} {{ get {{ return this._{x.Name}; }} set {{ this._{x.Name} = value; }} }}"))}

    public class {pascalCaseName}Output : BicepOutput {{
        {string.Join(Environment.NewLine, outputs.Select(x => @$"

        private {x.DotNetTypeName()}? _{x.Name};
        public {x.DotNetTypeName()}? {PascalCase(x.Name)} {{ get {{ return this._{x.Name}; }} set {{ this._{x.Name} = value; }} }}"))}

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

        return classTemplate;
    }

    private void GenerateAssembly(string[] classTemplates, string outputPath)
    {
        var classTemplate = string.Join(Environment.NewLine, classTemplates);
        classTemplate = $@"using BicepRunner;
using System.Collections.Generic;
{classTemplate}";

        var tree = SyntaxFactory.ParseSyntaxTree(classTemplate);

        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var references = new List<MetadataReference>();

        references.Add(MetadataReference.CreateFromFile(assemblyPath));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "netstandard.dll")));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Collections.dll")));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")));
        references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Private.CoreLib.dll")));

        //Contains some core type we inherit from
        var bicepRunner = typeof(IBicepRunner).GetTypeInfo().Assembly.Location;
        references.Add(MetadataReference.CreateFromFile(bicepRunner));
        if (_referenceTypesAssembly != null)
        {
            references.Add(MetadataReference.CreateFromFile(_referenceTypesAssembly!));
        }
        
        var compilation = CSharpCompilation.Create("BicepTypes")
            .WithOptions(
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                    nullableContextOptions: NullableContextOptions.Enable,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default))
            .AddReferences(references)
            .AddSyntaxTrees(tree);

        var path = Path.GetFullPath(Path.Combine(outputPath, $"BicepTypes.dll"));
        var compilationResult = compilation.Emit(path);
        if (!compilationResult.Success)
        {
            foreach (var issue in compilationResult.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error))
                Console.WriteLine(issue.ToString());
            throw new InvalidOperationException(
                $"Failed to generate compiled library: {compilationResult.Diagnostics}");
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        foreach (var issue in compilationResult.Diagnostics)
            Console.WriteLine(issue.ToString());
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Outputted assembly at {path}");
        Console.ResetColor();
    }

    static string PascalCase(string fileName)
    {
        var info = CultureInfo.CurrentCulture.TextInfo;
        return info.ToTitleCase(fileName)
            .Replace("-", "")
            .Replace(" ", "");
    }

}