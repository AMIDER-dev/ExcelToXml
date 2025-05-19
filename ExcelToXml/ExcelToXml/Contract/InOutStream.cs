using System;
using System.Collections.Generic;
using System.IO;

namespace Nipr.ExcelToXml.Contract;

public interface IDatatableStream : IDisposable
{
    Stream Stream { get; }
}

public interface ISampleXmlStream : IDisposable
{
    Stream Stream { get; }
}

public interface IXmlFilesStream : IDisposable
{
    ICollection <(string key, Stream)> Streams { get; }
}

public interface IXsdStream : IDisposable
{
    IEnumerable<(Uri ident, Stream)> Streams { get; }
    string Root { get; }
}

public interface IOutputStreams : IDisposable
{
    Stream GetStream(string key);
}
