using Nipr.ExcelToXml.Contract;

namespace Nipr.ExcelToXml.Console.Implements;
[RegistClass]
internal class Logger : ILogger
{
    public LogFilter LogFilter { get; set; } = LogFilter.Default;
    public void Message(string message)
    {
        if (LogFilter.HasFlag(LogFilter.Message))
        {
            System.Console.WriteLine(message);
        }
    }
    public void Error(string message)
    {
        if (LogFilter.HasFlag(LogFilter.Error))
        {
            System.Console.Error.WriteLine(message);
        }
    }
    public void Warning(string message)
    {
        if (LogFilter.HasFlag(LogFilter.Warning))
        {
            System.Console.Error.WriteLine(message);
        }
    }
    public void Info(string message)
    {
        if (LogFilter.HasFlag(LogFilter.Info))
        {
            System.Console.WriteLine(message);
        }
    }
}
