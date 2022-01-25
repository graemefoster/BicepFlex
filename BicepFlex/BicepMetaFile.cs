namespace BicepFlex;

public class BicepMetaFile
{
    public string FileName { get; }
    private readonly IEnumerable<BicepToken> _tokens;

    public BicepMetaFile(string fileName, byte[] hash, IEnumerable<BicepToken?> tokens)
    {
        FileName = fileName;
        _tokens = tokens.Where(x => x != null).Select(x => x!).ToArray();
        Hash = Convert.ToBase64String(hash);
    }

    public IEnumerable<BicepMetaFile> References => Array.Empty<BicepMetaFile>();
    public IEnumerable<BicepParameterToken> Parameters => _tokens.OfType<BicepParameterToken>();
    public IEnumerable<BicepOutputToken> Outputs => _tokens.OfType<BicepOutputToken>();
    public string Hash { get; }

    public bool PostProcess(IEnumerable<BicepMetaFile> files)
    {
        var madeInferences = false;
        foreach (var varToken in _tokens.OfType<BicepVarVariableReferenceToken>())
        {
            if (varToken.InferType(_tokens)) madeInferences = true;
        }

        return madeInferences;
    }
}