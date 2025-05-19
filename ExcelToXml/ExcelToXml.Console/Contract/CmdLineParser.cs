using System.Collections.Generic;

namespace Nipr.ExcelToXml.Console.Contract;

public interface ICmdLineParser
{
    bool Parse();
    IEnumerable<string> Usage();
}
public interface ICmdLineArguments
{
    string[] Args { get; }
}

public interface IOptions
{
    bool ApplyAction(params string[] opts);
    string Description { get; }
    (string lng, string shrt) Option { get; }

    sealed bool Is(string str) => Option.lng == str || Option.shrt == str;
    sealed string ToString() => $"{Option.lng,-18:s} ({Option.shrt,-2:s}) : {Description}";
}
