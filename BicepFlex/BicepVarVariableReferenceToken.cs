using System.Text.RegularExpressions;

namespace BicepFlex;

public class BicepVarVariableReferenceToken : BicepToken
{
    private static readonly Regex regex = new(@"^\s*var\s+([A-Za-z0-9_]*)\s*\=\s*([A-Za-z0-9_]*)\s*$");

    public BicepVarVariableReferenceToken(string variableName, string references)
    {
        VariableName = variableName;
        References = references;
    }

    public string VariableName { get; }
    public string References { get; }

    public static bool TryParse(IEnumerator<string> reader, out BicepVarVariableReferenceToken? token)
    {
        var line = reader.Current;
        var match = regex.Match(line);
        if (match.Success)
        {
            token = new BicepVarVariableReferenceToken(match.Groups[1].Value, match.Groups[2].Value);
            return true;
        }

        token = null;
        return false;
    }
}

public class BicepModuleReferenceToken : BicepToken
{
    private static readonly Regex regex = new(@"^\s*module\s+([A-Za-z0-9_]*)\s+\'([A-Za-z0-9_\.\/\-]*)\'");
    private static readonly Regex referenceRegex = new(@"^\s+([A-Za-z0-9_]*)\s*\:\s*([A-Za-z0-9_\.]*)\s*$");

    private BicepModuleReferenceToken(string variableName, string modulePath, List<(string, string)> parameters)
    {
        VariableName = variableName;
        ModulePath = modulePath;
        Parameters = parameters;
    }

    public string VariableName { get; }
    public string ModulePath { get; }
    public List<(string, string)> Parameters { get; }

    public static bool TryParse(IEnumerator<string> reader, out BicepModuleReferenceToken? token)
    {
        var line = reader.Current;
        var match = regex.Match(line);
        if (match.Success)
        {
            var moduleName = match.Groups[1].Value;
            var modulePath = match.Groups[2].Value;

            //look for variables and parameters going in:
            var openParenthesis = line.Contains('{') ? 1 : 0;
            var parameters = new List<(string, string)>();
            while (openParenthesis > 0 && reader.MoveNext())
            {
                var moduleLine = reader.Current;
                if (moduleLine.Contains('{')) openParenthesis++;
                if (moduleLine.Contains('}')) openParenthesis--;
                var referenceMatch = referenceRegex.Match(moduleLine);
                if (referenceMatch.Success)
                    parameters.Add((referenceMatch.Groups[1].Value, referenceMatch.Groups[2].Value));
            }

            if (openParenthesis != 0)
                throw new InvalidOperationException(
                    $"Failed to parse module. Finished with {openParenthesis} open curly brackets");

            token = new BicepModuleReferenceToken(moduleName, modulePath, parameters);
            return true;
        }

        token = null;
        return false;
    }
}