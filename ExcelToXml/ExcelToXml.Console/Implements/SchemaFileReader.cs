using Nipr.ExcelToXml.Console.Contract;
using Nipr.ExcelToXml.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nipr.ExcelToXml.Console.Implements;
[RegistClass]
internal class SchemaFileReader([FromKeyedServices("XsdFile")] ISingleInputPath xsdFile, [FromKeyedServices("XsdFolder")] ISingleInputPath xsdFolder, ILogger logger) : IXsdStream
{
    List<(Uri, Stream)>? loadedStream;
    public IEnumerable<(Uri ident, Stream)> Streams => loadedStream ??= LoadFiles().ToList();

    private string XsdSearchFolder => xsdFolder.Path ?? Path.GetDirectoryName(xsdFile.Path)!;

    public string Root => MakeUrn(xsdFile.Path!);

    public void Dispose() => loadedStream?.ForEach( i=>i.Item2.Dispose());

    private IEnumerable<(Uri, Stream)> LoadFiles()
    {
        foreach (var ident in Directory.EnumerateFiles(XsdSearchFolder, "*.xsd", SearchOption.AllDirectories))
        {
            logger.Info($"SchemaFile Load {ident}");
            var ms = new MemoryStream(File.ReadAllBytes(ident));
            yield return (new(MakeUrn(ident), UriKind.Absolute), ms);
        }
    }

    private string MakeUrn(string path)
    {
        var relatePath = Path.GetRelativePath(XsdSearchFolder, path);
        return $"urn:./{relatePath.Replace('\\', '/')}";
    }
}
