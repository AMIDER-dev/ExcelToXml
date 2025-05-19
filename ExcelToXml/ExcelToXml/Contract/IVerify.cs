using System.IO;
using System.Xml.Schema;

namespace Nipr.ExcelToXml.Contract;

public interface IVerify
{
    public bool VerifySchema();
    public bool VerifyXml(string ident, Stream stream);
}
