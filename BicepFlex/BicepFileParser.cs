using System.Security.Cryptography;
using System.Text;

namespace BicepFlex;

public class BicepFileParser
{
    public BicepMetaFile Parse(string fileName, string[] fileContents)
    {
        return new BicepMetaFile(
            Path.GetFileNameWithoutExtension(fileName), 
            SHA512.Create().ComputeHash(
                Encoding.UTF8.GetBytes(
                    string.Join(Environment.NewLine, fileContents))),
            GetTokens(fileContents.AsEnumerable()));
    }

    private IEnumerable<BicepToken> GetTokens(IEnumerable<string> lines)
    {
        var tokens = new List<BicepToken>();
        using var enumerator = lines.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var token = NextToken(enumerator);
            if (token != null) tokens.Add(token);
        }

        return tokens;
    }

    private BicepToken? NextToken(IEnumerator<string> fileReader)
    {
        bool more;
        do
        {
            if (BicepParameterToken.TryParse(fileReader, out var token)) return token;
            if (BicepOutputToken.TryParse(fileReader, out var token2)) return token2;
            if (BicepEnumToken.TryParse(fileReader, out var token3)) return token3;
            if (BicepVariableToken.TryParse(fileReader, out var token4)) return token4;
            if (BicepModuleReferenceToken.TryParse(fileReader, out var token5)) return token5;
            more = fileReader.MoveNext();
        } while (more);

        return null;
    }
    
}