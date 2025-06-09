using Nipr.ExcelToXml.Console.Contract;
using Nipr.ExcelToXml.Contract;
using System.Collections.Generic;

namespace Nipr.ExcelToXml.Console.Implements.FileHandle;

[RegistClass]
internal class PathChecker(
    [FromKeyedServices("DataTableA")]  ISingleInputPath dataA, [FromKeyedServices("DataTableB")]  ISingleInputPath dataB, [FromKeyedServices("SampleXML")]  ISingleInputPath xml,
    [FromKeyedServices("ElementDefine")]  ISingleInputPath defineTable, [FromKeyedServices("XsdFile")]  ISingleInputPath xsdFile , [FromKeyedServices("XmlFiles")] IMultipleFilePath xmlfiles,
    IOutputFile outFolder , ILogger logger) 
    : ICheckPrepare
{
    public bool IsReadyToRun => IsReadyToMakeXml || (IsReadyToReadXml && IsReadyToValidation);

    public bool IsReadyToReadXml => CheckReadExistsXmlOptionResult();

    public bool IsReadyToMakeXml => CheckMakeXmlOptionResult();

    public bool IsReadyToValidation => CheckValidationOptionResult();

    private bool CheckReadExistsXmlOptionResult()
    {
        logger.Info("実行前準備の状態確認 XmlFiles のパス :");
        foreach (var file in xmlfiles.Paths)
        {
            logger.Info(file);
        }
        return xmlfiles.Paths.Count > 0;
    }

    private bool CheckMakeXmlOptionResult()
    {
        logger.Info($"実行前準備の状態確認 DataTableA のパス : {dataA.Path}");
        logger.Info($"実行前準備の状態確認 DataTableB のパス : {dataB.Path}");
        logger.Info($"実行前準備の状態確認 ElementDfine のパス : {defineTable.Path}");
        logger.Info($"実行前準備の状態確認 SampleXML のパス : {xml.Path}");
        logger.Info($"実行前準備の状態確認 出力先フォルダのパス : {outFolder.OutFolder}");
        return dataA.Path != null && dataB.Path != null && xml.Path != null && defineTable.Path != null  && outFolder.OutFolder != null;
    }

    private bool CheckValidationOptionResult()
    {
        logger.Info($"実行前準備の状態確認 xsdファイルのパス : {xsdFile.Path}");
        return xsdFile.Path != null;
    }

}

[RegistClass(Name ="DataTableA")]
internal class ExcelFileA: ISingleInputPath
{
    public string? Path { get; set; }
}

[RegistClass(Name ="DataTableB")]
internal class ExcelFileB: ISingleInputPath
{
    public string? Path { get; set; }
}

[RegistClass(Name ="SampleXML")]
internal class BaseXMLFile: ISingleInputPath
{
    public string? Path { get; set; }
}

[RegistClass(Name ="ElementDefine")]
internal class ExcelFileDefine: ISingleInputPath
{
    public string? Path { get; set; }
}

[RegistClass(Name ="XsdFile")]
internal class XsdFile : ISingleInputPath
{
    public string? Path { get; set; }
}

[RegistClass(Name ="XsdFolder")]
internal class XsdFolder : ISingleInputPath
{
    public string? Path { get; set; }
}

[RegistClass(Name ="XmlFiles")]
internal class CheckXmlFiles : IMultipleFilePath
{
    public ICollection<string> Paths { get; set; } = [];
}

[RegistClass]
internal class OutFiles : IOutputFile
{
    public string? OutFolder { get; set; }
    public ICollection<string> Paths { get; } = [];
}
