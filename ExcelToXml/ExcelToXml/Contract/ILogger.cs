using System;

namespace Nipr.ExcelToXml.Contract;

[Flags]
public enum LogFilter
{
    Info = 1,
    Message = 2,
    Warning =4,
    Error =8,

    All =  Info | Message | Warning| Error,
    Default = Message | Warning| Error,  
}

public interface ILogger
{
    LogFilter LogFilter { get; set; }
    void Message(string message);
    void Error(string message);
    void Warning(string message);
    void Info(string message);
}
