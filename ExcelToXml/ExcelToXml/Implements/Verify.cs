using Nipr.ExcelToXml.Contract;
using System.IO;
using System.Xml.Linq;
using System.Xml.Resolvers;
using System.Xml.Schema;

namespace Nipr.ExcelToXml.Implements;
[RegistClass]
internal class Verify(IXsdStream xsdStream, ILogger logger): IVerify
{
    private XmlSchemaSet schemaSet = new();
    private bool hasError = false;

    public bool VerifySchema()
    {
        var resolver = new XmlPreloadedResolver();
        foreach (var (ident, stream) in xsdStream.Streams)
        {
            resolver.Add(ident, stream);
        }

        schemaSet = new() { XmlResolver = resolver };
        schemaSet.ValidationEventHandler += XsdVerifyCallback;
        schemaSet.Add(null, xsdStream.Root);
        schemaSet.Compile();
        return !hasError;
    }

    public bool VerifyXml(string ident, Stream stream)
    {
        hasError = false;
        var doc = XDocument.Load(stream);
        doc.Validate(schemaSet, DocumentVerifyCallback);
        if (hasError)
        {
            logger.Error($"検証NG: {ident}");
            return false;
        }
        else
        {
            logger.Message($"検証OK: {ident}");
            return true;
        }
    }

    private void XsdVerifyCallback(object? sender, ValidationEventArgs e)
    {
        switch(e.Severity)
        {
            case XmlSeverityType.Warning:
                logger.Warning($"XSD検証警告 : {e.Exception?.SourceUri} : {e.Exception?.LineNumber }  {e.Message}");
                return;
            case XmlSeverityType.Error:
                logger.Warning($"XSD検証エラー : {e.Exception?.SourceUri} : {e.Exception?.LineNumber} {e.Message}");
                hasError = true;
                return;
        }
    }

    private void DocumentVerifyCallback(object? sender, ValidationEventArgs e)
    {
        switch(e.Severity)
        {
            case XmlSeverityType.Warning:
                logger.Warning($"XML検証警告 : {e.Exception?.SourceUri} : {e.Exception?.LineNumber} {e.Message}");
                return;
            case XmlSeverityType.Error:
                logger.Warning($"XML検証エラー : {e.Exception?.SourceUri} : {e.Exception?.LineNumber} {e.Message}");
                hasError = true;
                return;
        }
    }


}

