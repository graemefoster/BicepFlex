using System.Collections.Concurrent;
using System.Reflection;

namespace BicepParser;

/// <summary>
/// Process a tree of Bicep files given an entry Uri
/// </summary>
public class BicepUriProcessor
{
    private readonly Uri _bicepRootUri;
    private readonly Uri _relativeEntryFileUri;
    private readonly string? _referenceTypesAssembly;
    private readonly string _fileSystemBasedRootUri;

    public BicepUriProcessor(Uri bicepRootUri, Uri relativeEntryFileUri, string? referenceTypesAssembly = null)
    {
        _bicepRootUri = bicepRootUri;
        _fileSystemBasedRootUri = $"c:{bicepRootUri.LocalPath}";
        _relativeEntryFileUri = relativeEntryFileUri;
        _referenceTypesAssembly = referenceTypesAssembly;
    }

    public async Task<(BicepMetaFile, BicepMetaFile[])> Process()
    {
        if (_bicepRootUri.Scheme.Contains("http") && !_relativeEntryFileUri.IsAbsoluteUri)
        {
            var allFiles = new ConcurrentDictionary<string, BicepMetaFile>();
            var client = new HttpClient();
            var parser = new BicepFileParser();

            async Task ProcessFile(Uri relativeToRootUri)
            {
                var requestUri = new Uri(_bicepRootUri, relativeToRootUri);
                if (allFiles.ContainsKey(requestUri.AbsoluteUri)) return;

                using var reader = new StreamReader(await client.GetStreamAsync(requestUri));
                var lines = new List<string>();
                while (!reader.EndOfStream)
                {
                    lines.Add((await reader.ReadLineAsync())!);
                }

                var file = parser.Parse(
                    _fileSystemBasedRootUri, //helps all the Path.xxxx functions work!
                    Path.GetRelativePath(_bicepRootUri.LocalPath, requestUri.LocalPath),
                    lines.ToArray());

                //in-case something else added it (tasks)
                allFiles.TryAdd(requestUri.AbsoluteUri, file);
                
                await Task.WhenAll(file.References.Select(x =>
                    ProcessFile(new Uri(x.ReferencedFileName, UriKind.Relative))));
            }

            await ProcessFile(_relativeEntryFileUri);

            var bicepMetaFiles = allFiles.Values.ToArray();
            new BicepFileSetPostProcessor().PostProcess(_fileSystemBasedRootUri, _referenceTypesAssembly, bicepMetaFiles);
            return (allFiles[new Uri(_bicepRootUri, _relativeEntryFileUri).AbsoluteUri], bicepMetaFiles);
        }

        throw new ArgumentException("Please provide a http(s) root Uri, and a relative entry file uri");
    }
}