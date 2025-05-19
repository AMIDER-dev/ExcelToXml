using System.IO;

internal static class TemplateReader
{
    private static readonly string TemplateFolder = $"{typeof(TemplateReader).Assembly.GetName().Name}.template";
    public static readonly string AttributeCS = GetStreamText($"{TemplateFolder}.Attribute.cs");
    public static readonly string RegisterCS = GetStreamText($"{TemplateFolder}.RegisterMethods.cs");
    static string GetStreamText(string name)
    {
        using var st = typeof(TemplateReader).Assembly.GetManifestResourceStream(name);
        using var reader = new StreamReader(st);
        return reader.ReadToEnd();
    }
}
