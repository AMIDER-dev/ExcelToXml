using Nipr.ExcelToXml.Console.Contract;
using Nipr.ExcelToXml.Contract;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nipr.ExcelToXml.Console.Implements;

internal class UnknownOpt(string opt, ILogger logger) : IOptions
{
    public string Description { get; } = "";
    public (string lng, string shrt) Option { get; } = ("", "");

    public bool ApplyAction(params string[] opts)
    {
        logger.Warning($"未知の引き数 {opt} が指定されています");
        return false;
    }
}

internal class dummyOption : IOptions
{
    public string Description { get; } = "";
    public (string lng, string shrt) Option { get; } = ("", "");

    public bool ApplyAction(params string[] opts) => true;
}

[RegistClass]
internal class CmdLineArgs : ICmdLineArguments
{
    public string[] Args => Environment.GetCommandLineArgs();
}

[RegistClass]
internal class CmdLineParser(IEnumerable<IOptions> options, ICmdLineArguments cmdline, ILogger logger) : ICmdLineParser
{
    public bool Parse()
    {
        if (cmdline.Args.Length > 0)
        {
            IOptions key = new dummyOption();

            // オプションを解析
            var optionList = cmdline.Args
                .GroupBy(i => key = options.FirstOrDefault(opt => opt.Is(i)) ?? (i.StartsWith('-') ? new UnknownOpt(i, logger) : key))
                .ToDictionary(i => i.Key, i => i.Where(j => !i.Key.Is(j)).ToArray());

            // 全てのオプションを評価する。
            return optionList.Aggregate(true, (pre, opt) => pre & opt.Key.ApplyAction(opt.Value));
        }
        return false;
    }

    public IEnumerable<string> Usage()
    {
        yield return "実行に必要な引き数が不足している、起動時引き数の内容ににエラーがあります";
        foreach(var opt in options)
        {
            yield return opt.ToString()!;
        }
    }
}
