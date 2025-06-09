namespace Nipr.ExcelToXml.Contract;

public interface IBootstrap
{
    bool Prepare();
    bool PrepareTidy();
    bool Run();
    bool RunTidy();
}

public interface IRunner
{
    bool Read();
    bool Convert();
    bool Write();
    bool ImportXml();
    bool Verify();
    void Tidy();
}
