using System.Reflection;

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
        var classes = new List<(string filename, string contents)>();
        var parse = new BicepFileParser();

        var allMetaFiles = await Task.WhenAll(Directory.GetFiles(_bicepRoot, "*.bicep", SearchOption.AllDirectories)
            .Select(async f =>
                parse.Parse(_bicepRoot, Path.GetRelativePath(_bicepRoot, f), await File.ReadAllLinesAsync(f))));

        //Add parents to the dependency tree
        foreach (var file in allMetaFiles)
        {
            file.InferTree(_bicepRoot, allMetaFiles);
        }

        //Try to infer types
        var referenceTypeAssembly = _referenceTypesAssembly == null ? null : Assembly.LoadFile(_referenceTypesAssembly);
        var keepGoing = true;
        while (keepGoing)
        {
            var madeInferences = false;
            foreach (var file in allMetaFiles)
            {
                if (file.InferTypes(_bicepRoot, allMetaFiles, referenceTypeAssembly)) madeInferences = true;
            }

            //If we made some inferences, do another pass to see if we can make some more
            keepGoing = madeInferences;
        }

        return allMetaFiles;
    }
}