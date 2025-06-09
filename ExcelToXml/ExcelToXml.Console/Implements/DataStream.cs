using Nipr.ExcelToXml.Console.Contract;
using Nipr.ExcelToXml.Contract;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nipr.ExcelToXml.Console.Implements;

internal abstract class AbstractSingleFileOpen(ISingleInputPath file, ILogger logger)
{
    private MemoryStream? fileStream = null;
    public Stream Stream => fileStream ??= FileRead();
    public void Dispose() => fileStream?.Dispose();
    private MemoryStream FileRead()
    {
        logger.Info($"ファイルの読込み <{file.Path!}>");
        return new(File.ReadAllBytes(file.Path!));
    }

}

[RegistClass(Name = "DataTableA")]
internal class DataStreamA([FromKeyedServices("DataTableA")]  ISingleInputPath file, ILogger logger) : AbstractSingleFileOpen(file, logger), IDatatableStream;

[RegistClass(Name = "DataTableB")]
internal class DataStreamB([FromKeyedServices("DataTableB")]  ISingleInputPath file, ILogger logger) : AbstractSingleFileOpen(file, logger), IDatatableStream;

[RegistClass(Name = "ElementDefine")]
internal class ElementDefineStream([FromKeyedServices("ElementDefine")]  ISingleInputPath file, ILogger logger) : AbstractSingleFileOpen(file, logger), IDatatableStream;

[RegistClass]
internal class SampleXMLStream([FromKeyedServices("SampleXML")]  ISingleInputPath file, ILogger logger) : AbstractSingleFileOpen(file, logger), ISampleXmlStream;

[RegistClass]
public class XmlFilesStream([FromKeyedServices("XmlFiles")] IMultipleFilePath files, ILogger logger) : IXmlFilesStream
{
    private List<(string, Stream)>? opendFile = null;

    public ICollection<(string key, Stream)> Streams => opendFile ??= InitStreams().ToList();

    public void Dispose()
    {
        if(opendFile != null)
        {
            foreach (var (_, st) in opendFile )
            {
                st.Dispose();
            }
            opendFile.Clear();
            opendFile = null;
        }
    }
    private IEnumerable<(string, Stream)> InitStreams()
    {
        foreach (var key in files.Paths)
        {
            logger.Info($"ファイルの読込み {key}");
            var buf = File.ReadAllBytes(key);
            yield return (key, new MemoryStream(buf));
        }
    }
}

[RegistClass]
internal class OutputFiles(IOutputFile outFile, ILogger logger) : IOutputStreams
{
    private readonly Dictionary<string, Stream> opendFile = [];

    public void Dispose()
    {
        foreach(var (_, stream) in opendFile)
        {
            stream.Dispose();
        }
    }

    public Stream GetStream(string key)
    {
        if( opendFile.TryGetValue(key, out var stream))
        {
            return stream;
        }
        logger.Info($"ディレクトリの作成 {outFile.OutFolder!}");
        Directory.CreateDirectory(outFile.OutFolder!);
        var path = Path.Combine(outFile.OutFolder!, key);
        logger.Info($"ファイルを書込み用にオープン {outFile.OutFolder!}");
        var filestream = File.Open(path, FileMode.Create);
        opendFile[key] = filestream;
        return filestream;
    }
}
