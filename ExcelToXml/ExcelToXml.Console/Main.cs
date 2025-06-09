using Nipr.ExcelToXml.Contract;
using System;

namespace Nipr.ExcelToXml.Console;

internal class ConsoleMain
{
    public static void Main(string[] args)
    {
        var provider = InitializeDI();

        var bootstrap = provider.GetService<IBootstrap>();

        ArgumentNullException.ThrowIfNull(nameof(bootstrap));

        _=  (bootstrap!.Prepare() || bootstrap.PrepareTidy()) && (bootstrap.Run() || bootstrap.RunTidy());

    }

    private static ServiceProvider InitializeDI()
    {
        var service = new ServiceCollection();

        ConsoleServiceRegister.RegisterClasses(service);
        ExportServiceRegister.RegisterClasses(service);

        return service.BuildServiceProvider();
    }
}

[GenerateRegister]
public partial class ConsoleServiceRegister;
