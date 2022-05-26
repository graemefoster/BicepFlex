using System.Security.Cryptography;
using System.Text;
using BicepParser.Tokens;

namespace BicepParser;

internal class BicepFileParser
{
    public BicepMetaFile Parse(string rootDirectory, string fileName, string[] fileContents)
    {
        var directory = Path.GetDirectoryName(fileName);
        var moduleName = Path.GetFileNameWithoutExtension(fileName);

        return new BicepMetaFile(
            directory,
            fileName,
            SHA512.Create().ComputeHash(
                Encoding.UTF8.GetBytes(
                    string.Join("\r\n", fileContents))),
            GetTokens(fileContents.AsEnumerable(), rootDirectory, directory!, moduleName));
    }

    private IEnumerable<BicepToken> GetTokens(IEnumerable<string> lines, string rootDirectory, string directory,
        string moduleName)
    {
        var tokens = new List<BicepToken>();
        using var enumerator = lines.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var token = NextToken(enumerator, rootDirectory, directory, moduleName);
            if (token != null) tokens.Add(token);
        }

        return tokens;
    }

    private BicepToken? NextToken(IEnumerator<string> fileReader, string rootDirectory, string directory,
        string moduleName)
    {
        bool more;
        do
        {
            //Ignore description tokens / comments
            if (BicepDescriptionToken.TryParse(fileReader))
            {
                more = fileReader.MoveNext();
                continue;
            }

            if (BicepCommentToken.TryParse(fileReader))
            {
                more = fileReader.MoveNext();
                continue;
            }

            if (BicepEmptyLineToken.TryParse(fileReader))
            {
                more = fileReader.MoveNext();
                continue;
            }

            if (BicepScopeToken.TryParse(fileReader, out var scopeToken)) return scopeToken;
            if (BicepParameterToken.TryParse(moduleName, fileReader, out var token)) return token;
            if (BicepOutputToken.TryParse(fileReader, out var token2)) return token2;
            if (BicepEnumToken.TryParse(moduleName, fileReader, out var token3)) return token3;
            if (BicepVariableToken.TryParse(fileReader, out var token4)) return token4;
            if (BicepModuleReferenceToken.TryParse(fileReader, rootDirectory, directory, out var token5)) return token5;

            more = fileReader.MoveNext();
            
        } while (more);

        return null;
    }
}