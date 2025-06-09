namespace  Nipr.ExcelToXml.Console.Contract;
public interface ICheckPrepare
{
    bool IsReadyToRun { get; }
    bool IsReadyToMakeXml { get; }
    bool IsReadyToValidation { get; }
}
