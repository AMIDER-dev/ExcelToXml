using System.Collections.Generic;
using System.IO;

namespace Nipr.ExcelToXml.Contract;

public interface IConverter
{
    Stream Convert(IEnumerable<(string id, string path)> pathes, IEnumerable<(string id, string value, string group)> values);
}
