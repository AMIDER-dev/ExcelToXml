using Nipr.ExcelToXml.Console.Contract;
using Nipr.ExcelToXml.Contract;
using System;
using System.IO;
using System.Linq;

namespace Nipr.ExcelToXml.Console.Implements.Options;

internal abstract class AbstractSingleFileOption(ILogger logger, ISingleInputPath dbFile)
{
    public string Description => $"{TargetName} のパスを指定する。";
    protected abstract string TargetName { get; }

    private bool CheckError(Func<bool> predict, string errMsg)
    {
        if (!predict())
        {
            logger.Error(errMsg);
            return false;
        }
        return true;
    }

    public bool ApplyAction(params string[] opts)
    {
        if (CheckError(() => dbFile.Path == null, $"{TargetName} が複数回指定されています。")
        && CheckError(() => opts.Length == 1, $"{TargetName}が複数指定されています。 [{string.Join(" ", opts)}]")
        && CheckError(() => File.Exists(opts[0]), $"指定された{TargetName} [{opts[0]}] が存在しません。"))
        {
            dbFile.Path = opts[0];
            return true;
        }
        return false;
    }
}


[RegistClass]
internal class DataTableA(ILogger logger, [FromKeyedServices("DataTableA")] ISingleInputPath dbFile ) : AbstractSingleFileOption(logger, dbFile), IOptions
{
    protected override string TargetName => "データテーブルA";
    public (string lng, string shrt) Option => ("--datatable_a", "-a");
}

[RegistClass]
internal class DataTableB(ILogger logger, [FromKeyedServices("DataTableB")] ISingleInputPath dbFile ) : AbstractSingleFileOption(logger, dbFile), IOptions
{
    protected override string TargetName => "データテーブルB";
    public (string lng, string shrt) Option => ("--datatable_b", "-b");
}

[RegistClass]
internal class SampleXML(ILogger logger,  [FromKeyedServices("SampleXML")] ISingleInputPath dbFile ) : AbstractSingleFileOption(logger, dbFile), IOptions
{
    protected override string TargetName => "サンプルXML";
    public (string lng, string shrt) Option => ("--sample_xml", "-s");
}

[RegistClass]
internal class ElementDefine(ILogger logger,  [FromKeyedServices("ElementDefine")] ISingleInputPath dbFile ) : AbstractSingleFileOption(logger, dbFile), IOptions
{
    protected override string TargetName => "要素名定義テーブル";
    public (string lng, string shrt) Option => ("--element_define", "-e");
}

[RegistClass]
internal class XsdFile(ILogger logger,  [FromKeyedServices("XsdFile")] ISingleInputPath xsdFile) : AbstractSingleFileOption(logger, xsdFile), IOptions
{
    protected override string TargetName => "XSDファイル";
    public (string lng, string shrt) Option => ("--xsd", "-x");
}

[RegistClass]
internal class XsdFolder(ILogger logger,  [FromKeyedServices("XsdFolder")] ISingleInputPath xsdFolder)  : IOptions
{
    public (string lng, string shrt) Option => ("--xsdfolder", "");

    public string Description => "XSDファイルが含まれるフォルダを指定します。指定がない場合は、--xsdで指定されたファイルのあるディレクトリが指定されたものとして扱います";

    public bool ApplyAction(params string[] opts)
    {
        if (xsdFolder.Path != null)
        {
            logger.Error("XSDフォルダが複数回指定されています。");
            return false;
        }
        if (opts.Length != 1)
        {
            logger.Error($"XSDフォルダが複数指定されています。 [{string.Join(" ", opts)}]");
            return false;
        }
        if (File.Exists(opts[0]))
        {
            logger.Error($"指定されたXSDフォルダのパスに、ファイルが存在します。 [{opts[0]}]");
            return false;
        }
        if (!Directory.Exists(opts[0]))
        {
            logger.Error($"指定されたXSDフォルダが存在しません。[{opts[0]}]");
            return false;
        }
        xsdFolder.Path = opts[0];
        return true;
    }
}

[RegistClass]
internal class XmlFiles(ILogger logger, [FromKeyedServices("XmlFiles")] IMultipleFilePath xmlfiles) : IOptions
{

    public (string lng, string shrt) Option => ("--xmlfiles", "");

    public string Description => "データテーブルからXMLを作成せず指定のXMLファイルを検証する。";

    public bool ApplyAction(params string[] opts) =>
        opts.All(path =>
        {
            if (File.Exists(path))
            {
                xmlfiles.Paths.Add(Path.GetFullPath(path));
                return true;
            }
            else
            {
                logger.Error($"指定された XmlFile [{path}] が存在しません。");
                return false;
            }
        });
}

[RegistClass]
internal class OutputPath(ILogger logger, IOutputFile outFile) : IOptions
{
    public (string lng, string shrt) Option => ("--output", "-o");

    public string Description => "作成したファイルを出力するフォルダを指定します。";

    public bool ApplyAction(params string[] opts)
    {
        if (outFile.OutFolder != null)
        {
            logger.Error("出力フォルダが複数回指定されています。");
            return false;
        }
        if (opts.Length != 1)
        {
            logger.Error($"出力フォルダが複数指定されています。 [{string.Join(" ", opts)}]");
            return false;
        }
        if (File.Exists(opts[0]))
        {
            logger.Error($"指定された出力フォルダのパスに、ファイルが存在します。 [{opts[0]}]");
            return false;
        }
        outFile.OutFolder = opts[0];
        return true;
    }
}

internal abstract class AbstractLogOptions(ILogger logger)
{
    protected abstract LogFilter RemoveValue { get; }
    protected abstract LogFilter AddValue { get; }

    public bool ApplyAction(params string[] opts)
    {
        logger.LogFilter &= ~RemoveValue;
        logger.LogFilter |= AddValue;
        if(opts.Length != 0)
        {
            logger.Error($"不要な文字列が指定されています。");
            return false;
        }
        return true;
    }

}

[RegistClass]
internal class LogNoWarning(ILogger logger) : AbstractLogOptions(logger), IOptions
{
    protected override LogFilter RemoveValue => LogFilter.Warning;
    protected override LogFilter AddValue => 0;
    public (string lng, string shrt) Option => ("--nowarning", "");
    public string Description => "警告メッセージを表示しません。";
}

[RegistClass]
internal class LogInfo(ILogger logger) : AbstractLogOptions(logger), IOptions
{
    protected override LogFilter RemoveValue => 0;
    protected override LogFilter AddValue => LogFilter.Info;
    public (string lng, string shrt) Option => ("--info", "");
    public string Description => "処理詳細を表示します。";
}
