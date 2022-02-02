using System.Globalization;
using System.Reflection;
using BicepFlex.Tokens;

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
        var classes = new List<(string filename, string contents)>();
        var parse = new BicepFileParser();

        var allMetaFiles = await Task.WhenAll(Directory.GetFiles(_bicepRoot, "*.bicep", SearchOption.AllDirectories)
            .Select(async f => parse.Parse(Path.GetRelativePath(_bicepRoot, f), await File.ReadAllLinesAsync(f))));

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
            classes.Add(($"{PascalCase(file.ModuleName)}.cs", GenerateBicepClass(file)));
        }

        await WriteCsFiles(classes.ToArray(), _bicepOutputPath);
    }

    static string GenerateBicepClass(BicepMetaFile file)
    {
        var inputs = file.Parameters;
        var outputs = file.Outputs;

        var pascalCaseName = PascalCase(Path.GetFileNameWithoutExtension(file.FileName));
        var classTemplate = @$"
using BicepRunner;

{string.Join(Environment.NewLine, inputs.OfType<BicepEnumToken>().Select(et => $@"
public enum {et.Name}Options {{
{string.Join(Environment.NewLine, et.Tokens.Select(etv => $"    {etv},"))}
}}
"))}

public class {pascalCaseName} : BicepTemplate<{pascalCaseName}.{pascalCaseName}Output> {{
    public override string FileName => ""{file.FileName}"";
    public override string FileHash => ""{file.Hash}"";

{string.Join(Environment.NewLine, inputs.Select(x => @$"
    private {x.DotNetTypeName()} _{x.Name} = default!;
    public {x.DotNetTypeName()} {PascalCase(x.Name)} {{ get {{ return this._{x.Name}; }} set {{ this._{x.Name} = value; }} }}
"))}

    public class {pascalCaseName}Output : BicepOutput {{
        {string.Join(Environment.NewLine, outputs.Select(x => @$"

        private {x.DotNetTypeName()} _{x.Name} = default!;
        public {x.DotNetTypeName()} {PascalCase(x.Name)} {{ get {{ return this._{x.Name}; }} set {{ this._{x.Name} = value; }} }}"))}

        public {pascalCaseName}Output(Dictionary<string, object> outputs) {{
            base.SetProperties(outputs);
        }}
    }}

    public override Dictionary<string, object> BuildParameters() {{
        var dictionary = new Dictionary<string, object>();
{string.Join(Environment.NewLine, inputs.Select(x => @$"        dictionary[""{x.Name}""] = new {{ value = this._{x.Name}}};"))}
        return dictionary;
    }} 

    public override {pascalCaseName}Output BuildOutput(Dictionary<string, object> outputs) {{
        return new {pascalCaseName}Output(outputs);
    }} 
}}
";

        return classTemplate;
    }

    private async Task WriteCsFiles((string filename, string contents)[] classTemplates, string outputPath)
    {
        await Task.WhenAll(classTemplates.Select(f =>
            File.WriteAllTextAsync(Path.Combine(outputPath, f.filename), f.contents)));
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Outputted files at {outputPath}");
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