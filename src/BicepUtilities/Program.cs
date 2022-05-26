// See https://aka.ms/new-console-template for more information

using System.Text;
using BicepParser;
var bicepPath = args[0];
var bicepOutputPath = args[1];

var output = new BicepDirectoryTreeProcessor(bicepPath);
var files = await output.Process();

var entry = files.Single(x => x.FileName == "stack/environment/main.bicep");
var diGraph = $@"
digraph ""{entry.FileName}"" {{
{Recurse(files, entry)}
}}
";

Console.WriteLine(diGraph);

string Recurse(BicepMetaFile[] allFiles, BicepMetaFile template)
{
    var sb = new StringBuilder();
    foreach (var child in template.References)
    {
        var childFile = allFiles.Single(x => x.FileName == child.ReferencedFileName);
        sb.AppendLine($"\"{template.FileName}\" -> \"{childFile.FileName}\"");
        sb.Append(Recurse(allFiles, childFile));
    }

    return sb.ToString();
}
