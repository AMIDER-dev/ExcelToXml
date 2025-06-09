using ClosedXML.Excel;
using Nipr.ExcelToXml.Contract;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nipr.ExcelToXml.Implements;

internal abstract class AbstractDataTableRader(ILogger logger)
{
    protected abstract string DescriptionName { get; }
    protected abstract int StartRowIndex { get; }
    protected abstract char GroupPrefix { get; }

    protected void CellTypeCheck(XLDataType type, int row, int col)
    {
        switch (type)
        {
            case XLDataType.Text:
            case XLDataType.Blank:
                return;
            case XLDataType.Error:
                logger.Warning($"{DescriptionName}  セル ( ROW {row} , COL {col} ) が 関数エラーです。");
                break;
            case XLDataType.Boolean:
            case XLDataType.DateTime:
            case XLDataType.Number:
            case XLDataType.TimeSpan:
                logger.Warning($"{DescriptionName}  セル ( ROW {row} , COL {col} ) が 文字列ではありません。");
                break;
        }
    }

    protected IEnumerable<(string key, string group)> ReadKeys(IXLWorksheet worksheet, int elemEndIndex)
    {
        // 1行目～Element Name End列の値を取得
        for (int rowIndex = StartRowIndex; rowIndex <= worksheet.LastRowUsed()!.RowNumber(); rowIndex++)
        {
            var elemNames = new List<string>();
            var group = new List<int>();

            for (int colIndex = 1; colIndex <= elemEndIndex; colIndex++)
            {
                logger.Info($"{DescriptionName} キー情報 {rowIndex} {colIndex} の読み出し");
                var cell = worksheet.Cell(rowIndex, colIndex);
                CellTypeCheck(cell.DataType, rowIndex, colIndex);

                // セルが結合されているか判定
                if (cell.IsMerged())
                {
                    logger.Info($"{DescriptionName}  キー情報 {rowIndex} {colIndex} セル結合");
                    var range = cell.MergedRange();

                    // 横に結合されている場合、左端でなければ、スキップ
                    if (cell.Address.ColumnNumber > range.FirstCell().Address.ColumnNumber)
                    {
                        continue; 
                    }

                    elemNames.Add(range.FirstCell().GetValue<string>());
                    group.Add(range.FirstRow().RowNumber());
                }
                else
                {
                    // セルの値を取得して加工
                    elemNames.Add(cell.GetValue<string>());
                    group.Add(rowIndex);
                }
            }

            var key = string.Join("/", elemNames);
            if (string.IsNullOrEmpty(key))
            {
                logger.Info($"{DescriptionName} キー情報{rowIndex} が空であるため読出しを終了");
                yield break;
            }

            if (elemNames.Count > 1)
            {
                yield return (key, string.Join('/', group.Select(n => $"{GroupPrefix}_{n}")));
            }
            else
            {
                yield return (key, "");
            }
        }
    }

    protected IEnumerable<string> ReadValues(IXLWorksheet worksheet, int colIndex)
    {
        // 1行目～Element Name End列の値を取得
        for (int rowIndex = StartRowIndex; rowIndex <= worksheet.LastRowUsed()!.RowNumber(); rowIndex++)
        {
            logger.Info($"{DescriptionName}  値情報{rowIndex} {colIndex} の読み出し");
            var cell = worksheet.Cell(rowIndex, colIndex);
            CellTypeCheck(cell.DataType, rowIndex, colIndex);

            // セルの値を取得して加工
            yield return cell.GetValue<string>();
        }
    }

}

[RegistClass(Name = "DataTableA")]
internal class DataTableAReader([FromKeyedServices("DataTableA")] IDatatableStream file, ILogger logger) : AbstractDataTableRader(logger), IDatatableReader
{
    private const int ROWINDEX_TABLE_START = 3;
    private const string ELEMENT_NAME_END_CELL = "B1";

    protected override string DescriptionName => "DataTabe A";
    protected override int StartRowIndex => ROWINDEX_TABLE_START;
    protected override char GroupPrefix => 'A';

    public IEnumerable<(string key, IEnumerable<(string id, string value, string group)> values)> GetValues()
    {
        logger.Info($"{DescriptionName} の読み出し開始");
        using var workbook = new XLWorkbook(file.Stream);
        var worksheet = workbook.Worksheets.Worksheet(1);
        var cellVal = worksheet.Cell(ELEMENT_NAME_END_CELL).Value;
        int elemEndIndex = worksheet.Column((string)cellVal).ColumnNumber();

        var keys = ReadKeys(worksheet, elemEndIndex);
        var values = ReadValues(worksheet, elemEndIndex + 1);
        yield return ("all", keys.Zip(values, (key, value) => (key.key, value, key.group)));
    }

}

[RegistClass(Name = "DataTableB")]
internal class DataTableBReader([FromKeyedServices("DataTableB")] IDatatableStream file, ILogger logger) : AbstractDataTableRader(logger), IDatatableReader
{
    private const int ROWINDEX_TABLE_START = 4;
    private const string ELEMENT_NAME_END_CELL = "B1";

    protected override string DescriptionName => "DataTabe B";
    protected override int StartRowIndex => ROWINDEX_TABLE_START;
    protected override char GroupPrefix => 'B';

    public IEnumerable<(string key, IEnumerable<(string id, string value, string group)> values)> GetValues()
    {
        logger.Info($"{DescriptionName}  の読み出し開始");
        using var workbook = new XLWorkbook(file.Stream);
        var worksheet = workbook.Worksheets.Worksheet(1);

        var cellVal = worksheet.Cell(ELEMENT_NAME_END_CELL).Value;
        int elemEndIndex = worksheet.Column((string)cellVal).ColumnNumber();

        var keys = ReadKeys(worksheet, elemEndIndex).ToList();

        for (int colIndex = elemEndIndex + 1; colIndex <= worksheet.LastColumnUsed()!.ColumnNumber(); colIndex++)
        {
            var fileName = worksheet.Cell(2, colIndex).GetValue<string>();
            var values = ReadValues(worksheet, colIndex);

            yield return (fileName, keys.Zip(values, (key, value) => (key.key, value, key.group)));
        }
    }
}

[RegistClass]
internal class ElementDefineReader([FromKeyedServices("ElementDefine")] IDatatableStream file, ILogger logger) : IDefineTableReader
{
    public IEnumerable<(string id, string path)> GetPathes() => Read(file.Stream);

    protected void CellTypeCheck(XLDataType type, int row, int col)
    {
        switch (type)
        {
            case XLDataType.Text:
            case XLDataType.Blank:
                return;
            case XLDataType.Error:
                logger.Warning($"ElementDefine セル ( ROW {row} , COL {col} ) が 関数エラーです。");
                break;
            case XLDataType.Boolean:
            case XLDataType.DateTime:
            case XLDataType.Number:
            case XLDataType.TimeSpan:
                logger.Warning($"ElementDefine セル ( ROW {row} , COL {col} ) が 文字列ではありません。");
                break;
        }
    }

    public IEnumerable <(string id, string path)> Read(Stream fs)
    {
        logger.Info($"ElementDefine の読み出し開始");

        using var workbook = new XLWorkbook(fs);
        var worksheet = workbook.Worksheets.Worksheet(1);
        for (int rowIndex = 2; rowIndex <= worksheet.LastRowUsed()!.RowNumber(); rowIndex++)
        {
            logger.Info($"ElementDefine キー情報{rowIndex} 1 の読み出し");
            var name = worksheet.Cell(rowIndex, 1).GetValue<string>();
            CellTypeCheck(worksheet.Cell(rowIndex, 1).DataType, rowIndex, 1);
            if (string.IsNullOrEmpty(name))
            {
                logger.Info($"ElementDefine キー情報{rowIndex} 1 が空であるため読出しを終了");
                yield break;
            }
            logger.Info($"ElementDefine 値情報{rowIndex} 2 の読み出し");
            var path = worksheet.Cell(rowIndex, 2).GetValue<string>();
            CellTypeCheck(worksheet.Cell(rowIndex, 2).DataType, rowIndex, 2);

            yield return (name, path);
        }
    }
}

