using Nipr.ExcelToXml.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nipr.ExcelToXml.Implements;
[RegistClass]
internal class Runner( ILogger logger, IDataReader reader, IConverter converter, IVerify verify, IXmlFilesStream xmlStream, IOutputStreams outputStreams ) : IRunner
{
    private List<(string key, Stream stream)> Converted = [];

    public bool Read()
    {
        logger.Info("全体：読込み処理開始");
        try
        {
            return reader.ReadAll();
        }
        catch(Exception e)
        {
            logger.Error($"読込み中にエラーが発生しました。{e.Message}");
            return false;
        }
    }

    public bool Convert()
    {
        logger.Info("全体：XML化開始");
        try
        {
            foreach (var (outputId, values) in reader.GetValues())
            {
                logger.Info($"全体：XML化処理開始 {outputId}");
                var result = converter.Convert(reader.GetPathes(), values);
                logger.Info($"全体：XML化処理終了 {outputId}");
                Converted.Add((outputId, result));
            }
            return true;
        }
        catch(Exception e)
        {
            logger.Error($"変換処理実行中にエラーが発生しました。{e.Message}");
            return false;
        }
    }

    public bool Verify()
    {
        logger.Info("全体：検証処理開始");
        try
        {
            if (verify.VerifySchema())
            {
                return Converted.All(i =>
                {
                    var (ident, stream) = i;
                    logger.Info($"XML の ValidationCheck {ident}");
                    stream.Seek(0, SeekOrigin.Begin);
                    return verify.VerifyXml(ident, stream);
                });
            }
            return false;
        } catch (Exception e)
        {
            logger.Error($"検証処理実行中にエラーが発生しました。{e.Message}");
            return false;
        }
    }

    public bool Write()
    {
        logger.Info("全体：書込み処理開始");
        try
        {
            foreach (var (outputId, stream) in Converted)
            {
                logger.Info($"全体：書込み処理 {outputId}");
                using (var output = outputStreams.GetStream(outputId)) // Dispose 後にログを出すため、nested usingを使う。
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(output);
                }
                logger.Info($"全体：書込み処理終了 {outputId}");
            }
            return true;
        }catch(Exception e)
        {
            logger.Error($"書込み処理実行中にエラーが発生しました。{e.Message}");
            return false;
        }
    }

    public void Tidy()
    {
        logger.Info($"全体：終了処理");
        foreach(var (_, stream) in Converted)
        {
            stream.Dispose();
        }
        Converted.Clear();
    }

    public bool ImportXml()
    {
        try
        {
            logger.Info($"全体：既存XMLファイル読込み開始");
            Converted.AddRange(xmlStream.Streams);
            logger.Info($"全体：既存XMLファイル読込み終了");
            return true;
        }
        catch (Exception e)
        {
            logger.Error($"既存XMLファイル読込み中にエラーが発生しました。{e.Message}");
            return false;
        }
    }
}
