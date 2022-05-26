namespace BicepParser;

public class BicepDirectoryTreeProcessor
{
    private readonly string _bicepRoot;
    private readonly string? _referenceTypesAssembly;

    public BicepDirectoryTreeProcessor(string bicepRoot, string? referenceTypesAssembly = null)
    {
        _bicepRoot = bicepRoot;
        _referenceTypesAssembly = referenceTypesAssembly;
    }

    public async Task<BicepMetaFile[]> Process()
    {
        var parse = new BicepFileParser();

        var allMetaFiles = await Task.WhenAll(Directory.GetFiles(_bicepRoot, "*.bicep", SearchOption.AllDirectories)
            .Select(async f =>
                parse.Parse(_bicepRoot, Path.GetRelativePath(_bicepRoot, f), await File.ReadAllLinesAsync(f))));
        new BicepFileSetPostProcessor().PostProcess(_bicepRoot, _referenceTypesAssembly, allMetaFiles);
        return allMetaFiles;
    }
}