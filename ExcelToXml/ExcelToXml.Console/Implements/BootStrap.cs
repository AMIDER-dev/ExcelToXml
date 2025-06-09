using  Nipr.ExcelToXml.Console.Contract;
using Nipr.ExcelToXml.Contract;

namespace Nipr.ExcelToXml.Console.Implements;

[RegistClass]
internal class BootStrap(ILogger logger, ICmdLineParser parser, ICheckPrepare checker, IRunner runner) : IBootstrap
{
    public bool Prepare()
    {
        logger.Info("処理開始前の事前チェック開始");
        return parser.Parse() && checker.IsReadyToRun;
    }

    public bool PrepareTidy()
    {
        logger.Info("処理開始前の事前チェックの終了処理");
        foreach (var line in parser.Usage())
        {
            logger.Warning(line);
        }
        return false;
    }

    public bool Run()
    {
        logger.Info("本来処理の開始");
        var result =
            (checker.IsReadyToMakeXml
                ? (runner.Read() && runner.Convert() && runner.Write())
                : runner.ImportXml())
            &&
            (!checker.IsReadyToValidation || runner.Verify());
        logger.Info("本来処理の終了");
        runner.Tidy();
        return result;
    }

    public bool RunTidy() => true;
}
