using System.Reflection;
using BicepParser.Tokens;

namespace BicepParser;

public class BicepMetaFile
{
    public string ModuleName { get; }
    public string? Directory { get; }
    public string FileName { get; }
    public string Scope { get; }

    private readonly IEnumerable<BicepToken> _tokens;
    private readonly HashSet<BicepMetaFile> _referencedBy = new();

    public BicepMetaFile(string? directory, string fileName, byte[] hash, IEnumerable<BicepToken?> tokens)
    {
        Directory = directory;
        FileName = fileName.Replace(Path.DirectorySeparatorChar, '/');
        ModuleName = Path.GetFileNameWithoutExtension(fileName);
        _tokens = tokens.Where(x => x != null).Select(x => x!).ToArray();
        Hash = Convert.ToBase64String(hash);
        Scope = tokens.OfType<BicepScopeToken>().SingleOrDefault()?.Scope ?? "resourceGroup";
    }

    public IEnumerable<BicepMetaFile> ReferencedBy => _referencedBy;
    public IEnumerable<BicepModuleReferenceToken> References => _tokens.OfType<BicepModuleReferenceToken>();
    public IEnumerable<BicepParameterToken> Parameters => _tokens.OfType<BicepParameterToken>();
    public IEnumerable<BicepOutputToken> Outputs => _tokens.OfType<BicepOutputToken>();
    public string Hash { get; }

    public bool InferTypes(string rootPath, IEnumerable<BicepMetaFile> files, Assembly? referenceTypeAssembly)
    {
        var madeInferences = false;

        foreach (var varToken in _tokens.OfType<BicepVariableToken>())
        {
            if (varToken.InferType(_tokens, referenceTypeAssembly)) madeInferences = true;
        }

        //Look for inputs of type object and see if we can work out what the parameter type should be:
        foreach (var moduleReference in _tokens.OfType<BicepModuleReferenceToken>())
        {
            if (moduleReference.InferType(_tokens, referenceTypeAssembly))
            {
                var fullpath = Path
                    .GetRelativePath(rootPath, Path.Combine(rootPath, moduleReference.ReferencedFileName))
                    .Replace(Path.DirectorySeparatorChar, '/');
                var module = files.Single(x => x.FileName == fullpath);
                module.InferTypes(moduleReference);
            }
        }

        return madeInferences;
    }

    /// <summary>
    /// Look for more specific parameter information from a file that invokes this one
    /// </summary>
    /// <param name="referencingBicepFile"></param>
    private void InferTypes(BicepModuleReferenceToken referencingBicepFile)
    {
        foreach (var parameter in referencingBicepFile.Parameters)
        {
            if (parameter.InferredType != null)
            {
                Console.WriteLine(
                    $"Specific type discovered for parameter {parameter.Name} - {parameter.InferredType}");
                var parameterToken =
                    _tokens.OfType<BicepParameterToken>().SingleOrDefault(x => x.Name == parameter.Name);
                if (parameterToken == null)
                {
                    Console.WriteLine(
                        $"WARNING: parameter {parameter.Name} passed from {referencingBicepFile.ReferencedFileName} cannot be found in: {this.FileName}");
                    return;
                }

                if (parameterToken.CustomType == null)
                {
                    Console.WriteLine(
                        $"Changing parameter {parameterToken.Name} to have inferred type: {parameter.InferredType}");
                    parameterToken.CustomType = parameter.InferredType;
                }
            }
        }
    }

    private void ModuleReferencedBy(BicepMetaFile otherFile)
    {
        _referencedBy.Add(otherFile);
    }

    internal void InferTree(string rootPath, IEnumerable<BicepMetaFile> files)
    {
        foreach (var moduleReference in _tokens.OfType<BicepModuleReferenceToken>())
        {
            var fullpath = Path
                .GetRelativePath(rootPath, Path.Combine(rootPath, moduleReference.ReferencedFileName))
                .Replace(Path.DirectorySeparatorChar, '/');
            var module = files.Single(x => x.FileName == fullpath);
            module.ModuleReferencedBy(this);
        }
    }
}