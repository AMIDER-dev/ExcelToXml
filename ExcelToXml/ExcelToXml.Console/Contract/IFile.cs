
using System.Collections.Generic;

namespace  Nipr.ExcelToXml.Console.Contract;

public interface ISingleInputPath
{
    string? Path { get; set; }
}

public interface IMultipleFilePath
{
    ICollection<string> Paths { get; set; }
}

public interface IOutputFile
{
    string? OutFolder { get; set; }
    ICollection<string>? Paths{ get; }
}
