﻿// See https://aka.ms/new-console-template for more information

using System.Text;
using BicepParser;

// var bicepRoot = new Uri(args[0], UriKind.Absolute);
// var bicepEntry = new Uri(args[1], UriKind.Relative);
//
// var output = new BicepUriProcessor(bicepRoot, bicepEntry);
// var result = await output.Process();
// var entry = result.Item1;
// var files = result.Item2;

var files = await new BicepDirectoryTreeProcessor(args[0]).Process();
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
