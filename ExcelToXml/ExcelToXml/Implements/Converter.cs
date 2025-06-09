using Nipr.ExcelToXml.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Nipr.ExcelToXml.Implements;

[RegistClass]
internal class MakeXml(ISampleXmlStream sampleXml, ILogger logger) : IConverter
{
    private static string AddDefaultNamespaceTag(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return "";
        }
        if (name.Contains(':') || name == ".")
        {
            return name;
        }
        else
        {
            return $"DEFNS:{name}";
        }
    }

    private XmlNamespaceManager ReadNameSpace(XDocument doc)
    {
        logger.Info("SampleXML の名前空間読み出し");
        var nav = doc.Root!.CreateNavigator();
        var nms = new XmlNamespaceManager(nav.NameTable);
        var dict = nav.GetNamespacesInScope(XmlNamespaceScope.All);
        foreach (var (key, value) in dict)
        {
            logger.Info($"名前空間  {key} : {value}");
            nms.AddNamespace(key, value);
        }
        return nms;
    }

    private XElement? FindElement(XElement elem, string path, XmlNamespaceManager nms)
    {
        logger.Info($"エレメントの探索 {path}");
        var splited = path.Split("/");
        var changedpath = string.Join("/", splited.Select(AddDefaultNamespaceTag));

        return changedpath.Length == 0 ? elem : elem.XPathSelectElement(changedpath, nms);
    }

    private XElement? WriteElement(XElement elem, string path, string? value, XmlNamespaceManager nms, bool needCopy)
    {
        var element = FindElement(elem, path, nms);
        if (element != null)
        {
            if (needCopy)
            {
                logger.Info($"エレメントのコピーを作成");
                var newElement = new XElement(element);
                element.AddAfterSelf(newElement);
                element = newElement;
            }
            if (value != null)
            {
                logger.Info($"エメントに値を設定");
                element.Value = value;
            }
        }
        else
        {
            logger.Warning($"対象エレメントなし {path}");
        }
        return element;
    }

    private void RemoveEmptyElement(XDocument doc)
    {
        logger.Info($"空エメントの削除開始");
        var emptyElements = doc.Descendants().Where(e => string.IsNullOrEmpty(e.Value) && !e.HasElements).ToList();
        while (emptyElements.Count > 0)
        {
            foreach (var element in emptyElements)
            {
                logger.Info($"{element.Name} を削除");
                element.Remove();
            }
            emptyElements = doc.Descendants().Where(e => string.IsNullOrEmpty(e.Value) && !e.HasElements).ToList();
        }
    }

    private static string ReadStreamToEnd(Stream st)
    {
        using var reader = new StreamReader(st);
        return reader.ReadToEnd();
    }

    private string? BaseXmlString { get; set; } = null;

    public Stream Convert(IEnumerable<(string id, string path)> pathes, IEnumerable<(string id, string value, string group)> values)
    {
        logger.Info($"SampleXML を読込み");
        var xdoc = XDocument.Parse(BaseXmlString ??= ReadStreamToEnd(sampleXml.Stream));
        var nms = ReadNameSpace(xdoc);
        nms.PushScope();
        nms.AddNamespace("DEFNS", nms.DefaultNamespace);

        logger.Info($"階層化指定のない要素の変換を開始");
        var flatValues = values.Where(x => string.IsNullOrEmpty(x.group)).ToList();
        foreach (var (id, value, group) in flatValues)
        {
            foreach (var (_, xpath) in pathes.Where(i => i.id == id))
            {
                WriteElement(xdoc.Root!, xpath, value, nms, flatValues.Count(i => i.id == id) != 1 );
            }
        }

        logger.Info($"階層化指定のある要素の変換を開始");
        var nestedValues = values.Where(x => !string.IsNullOrEmpty(x.group)).Select(i => (i.id, i.value, group: i.group.Split('/'))).GroupBy(i => i.group[0]).ToList();
        foreach (var setitem in nestedValues)
        {
            ApplyNestedElement(xdoc.Root!, pathes, setitem, nms, 0);
        }

        RemoveEmptyElement(xdoc);

        logger.Info($"変換を終了");
        nms.PopScope();
        var ms = new MemoryStream();
        xdoc.Save(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    private void ApplyNestedElement(XElement root , IEnumerable<(string id, string path)> pathes, IEnumerable<(string id, string value, string[] group)> values, XmlNamespaceManager nms,  int index) 
    {
        if (values.Any())
        {
            var first = values.First();
            var splitid = first.id.Split('/');
            if (splitid.Length == index+1)
            {
                foreach (var (id, value, _) in values)
                {
                    foreach (var (_, xpath) in pathes.Where(i => i.id == id))
                    {
                        var newroot = WriteElement(root, $"./{xpath}", value, nms, true);
                    }
                }
            }
            else
            {
                var searchId = string.Join('/', splitid[0..(index+1)]);
                logger.Info($"親要素 {searchId} の作成");
                foreach (var (_, xpath) in pathes.Where(i => i.id == searchId))
                {
                    var modifyPath = $"./{(index == 0 ? "/" +string.Join('/', xpath.Split('/')[2..]) : xpath)}";

                    // 最上位要素の場合、XPathの指定を補正する。
                    var newroot = WriteElement(root, modifyPath, null, nms, true);

                    if (newroot != null)
                    {
                        foreach (var next in values.GroupBy(i => i.group[index]))
                            ApplyNestedElement(newroot, pathes, next, nms, index + 1);
                    }
                }
            }
        }
    }
}
