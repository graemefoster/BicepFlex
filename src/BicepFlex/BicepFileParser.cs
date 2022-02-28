using System.Security.Cryptography;
using System.Text;
using BicepFlex.Tokens;

namespace BicepFlex;

internal class BicepFileParser
{
    public BicepMetaFile Parse(string fileName, string[] fileContents)
    {
        var directory = Path.GetDirectoryName(fileName);
        var moduleName = Path.GetFileNameWithoutExtension(fileName);
        
        return new BicepMetaFile(
            directory,
            fileName,
            SHA512.Create().ComputeHash(
                Encoding.UTF8.GetBytes(
                    string.Join("\r\n", fileContents))),
            GetTokens(fileContents.AsEnumerable(), directory!, moduleName));
    }

    private IEnumerable<BicepToken> GetTokens(IEnumerable<string> lines, string directory, string moduleName)
    {
        var tokens = new List<BicepToken>();
        using var enumerator = lines.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var token = NextToken(enumerator, directory, moduleName);
            if (token != null) tokens.Add(token);
        }

        return tokens;
    }

    private BicepToken? NextToken(IEnumerator<string> fileReader, string directory, string moduleName)
    {
        bool more = true;
        do
        {
            //Ignore description tokens / comments
            if (BicepDescriptionToken.TryParse(fileReader)) more = fileReader.MoveNext();
            if (more && BicepCommentToken.TryParse(fileReader)) more = fileReader.MoveNext();
            if (more && BicepEmptyLineToken.TryParse(fileReader)) more = fileReader.MoveNext();

            if (more)
            {
                if (BicepParameterToken.TryParse(moduleName, fileReader, out var token)) return token;
                if (BicepOutputToken.TryParse(fileReader, out var token2)) return token2;
                if (BicepEnumToken.TryParse(moduleName, fileReader, out var token3)) return token3;
                if (BicepVariableToken.TryParse(fileReader, out var token4)) return token4;
                if (BicepModuleReferenceToken.TryParse(fileReader, directory, out var token5)) return token5;
                more = fileReader.MoveNext();
            }
            
        } while (more);

        return null;
    }
    
}