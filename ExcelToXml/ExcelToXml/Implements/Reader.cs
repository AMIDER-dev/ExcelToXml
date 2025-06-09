
using DocumentFormat.OpenXml.Office.CustomUI;
using Nipr.ExcelToXml.Contract;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks.Dataflow;

namespace Nipr.ExcelToXml.Implements;

[RegistClass]
internal class DataReader( [FromKeyedServices("DataTableA")] IDatatableReader DataTableA, [FromKeyedServices("DataTableB")] IDatatableReader DataTableB, IDefineTableReader DefineTable, ILogger logger) : IDataReader
{
    List<(string key, List<(string id, string value, string group)> values)> TableAItems = [];
    List<(string key, List<(string id, string value, string group)> values)> TableBItems = [];
    List<(string id, string path)> ElementPath = [];

    public IEnumerable<(string id, string path)> GetPathes() => ElementPath;

    public IEnumerable<(string key, IEnumerable<(string id, string value, string group)> values)> GetValues()
    {
        var commonValues = TableAItems.First().values;
        foreach (var (key, values) in TableBItems)
        {
            yield return (key, commonValues.Concat(values));
        }
    }

    public bool ReadAll()
    {
        logger.Info("Excelファイルの読み出し開始");
        TableAItems = DataTableA.GetValues().Select(r => (r.key, r.values.ToList())).ToList();
        TableBItems = DataTableB.GetValues().Select(r => (r.key, r.values.ToList())).ToList();
        ElementPath = DefineTable.GetPathes().ToList();

        var hasError = false;

        var idlist = ElementPath.Select(i => i.id).ToList();
        foreach(var id in idlist)
        {
            if (id.Contains('/'))
            {
                var split = id.Split('/');
                for (var i = 1; i < split.Length; i++) {
                    var idpart = string.Join('/', split[0..i]);
                    if(!idlist.Contains(idpart))
                    {
                        logger.Error($"ElementDefine に 「{idpart}」 が定義されていません。");
                        hasError = true;
                    }
                }
            }
        }

        var useidlist = TableAItems.Concat(TableBItems).SelectMany(t => t.values).Select(t => t.id).Distinct();
        foreach(var id in TableAItems.Concat(TableBItems).SelectMany(t => t.values).Select(t => t.id).Distinct())
        {
            if (idlist.Contains(id))
            {
                continue;
            }
            logger.Error($"ElementDefine に「{id}」が定義されていません。");
            hasError = true;
        }

        return !hasError;
    }
}
