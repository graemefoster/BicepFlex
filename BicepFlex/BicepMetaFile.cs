namespace BicepFlex;

public class BicepMetaFile
{
    private readonly IEnumerable<BicepToken> _tokens;

    public BicepMetaFile(IEnumerable<BicepToken> tokens)
    {
        _tokens = tokens;
    }

    public IEnumerable<BicepMetaFile> References => Array.Empty<BicepMetaFile>();
    public IEnumerable<BicepParameterToken> Parameters => _tokens.OfType<BicepParameterToken>();
    public IEnumerable<BicepOutputToken> Outputs => _tokens.OfType<BicepOutputToken>();
}