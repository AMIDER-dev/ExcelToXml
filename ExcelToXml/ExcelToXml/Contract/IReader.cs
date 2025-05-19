using System.Collections.Generic;

namespace Nipr.ExcelToXml.Contract;

public interface IDatatableReader
{
    IEnumerable<(string key, IEnumerable<(string id, string value, string group)> values)> GetValues();
}

public interface IDefineTableReader 
{
    IEnumerable<(string id, string path)> GetPathes();
}

public interface IDataReader
{
    bool ReadAll();

    IEnumerable<(string id, string path)> GetPathes();
    IEnumerable<(string key, IEnumerable<(string id, string value, string group)> values)> GetValues();
}
