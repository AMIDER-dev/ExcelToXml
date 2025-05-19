using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public enum ServiceLifetime
{
    Singleton = 0,
    Scoped = 1,
    Transient = 2
}

[Generator(LanguageNames.CSharp)]
public partial class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Attribute ファイルの作成 (固定ファイル)
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource("__Generate_Attribute_.cs", TemplateReader.AttributeCS));

        var RegistClassesProvider = context.SyntaxProvider.CreateSyntaxProvider(
            (node, _) => node is ClassDeclarationSyntax cls && CheckRegisterAttribute(cls),
            (context, _) => context.SemanticModel.GetDeclaredSymbol(context.Node) as INamedTypeSymbol);
        var GenerateTargetProvider = context.SyntaxProvider.CreateSyntaxProvider(
            (node, _) => node is ClassDeclarationSyntax cls && CheckTargetAttribute(cls),
            (context, _) => context.SemanticModel.GetDeclaredSymbol(context.Node) as INamedTypeSymbol);
        var collectProvider = GenerateTargetProvider.Combine(RegistClassesProvider.Collect());

        context.RegisterSourceOutput(collectProvider, GenerateRegisterMethod);
    }

    private static bool CheckRegisterAttribute(ClassDeclarationSyntax node)
        => node.AttributeLists.SelectMany(x => x.Attributes).Any(x => x.Name.ToString() == "RegistClass" || x.Name.ToString() == "RegistClassAttribute");
    private static bool CheckTargetAttribute(ClassDeclarationSyntax node)
        => node.AttributeLists.SelectMany(x => x.Attributes).Any(x => x.Name.ToString() == "GenerateRegister" || x.Name.ToString() == "GenerateRegisterAttribute");

    private static (string?, ServiceLifetime) GetAttribute(INamedTypeSymbol node)
    {
        // 必ず一つある。
        var attr = node.GetAttributes().Single(a => a.AttributeClass?.Name == "RegistClass" || a.AttributeClass?.Name == "RegistClassAttribute");

        ServiceLifetime lifetime = ServiceLifetime.Scoped;

        if (attr.NamedArguments.FirstOrDefault(a => a.Key == "Lifetime").Value.Value is ServiceLifetime value)
        {
            lifetime = value;
        }
        if (attr.NamedArguments.FirstOrDefault(a => a.Key == "Name").Value.Value is string name)
        {
            return (name, lifetime);
        }
        return (null, lifetime);
    }

    private static IEnumerable<string> GetRegisterLines(IEnumerable<INamedTypeSymbol?> types)
    {
        foreach (var type in types)
        {
            if (type != null)
            {
                var name = $"{type.ContainingNamespace.ToString()}.{type.Name}";
                var (tag, lifetime) = GetAttribute(type);

                foreach (var intf in type.AllInterfaces)
                {
                    var ifname = $"{intf.ContainingNamespace.ToString()}.{intf.Name}";

                    var registerFunc = (tag == null, lifetime) switch
                    {
                        (true, ServiceLifetime.Singleton) => $"AddSingleton(typeof({ifname}), typeof({name}))",
                        (true, ServiceLifetime.Scoped) => $"AddScoped(typeof({ifname}), typeof({name}))",
                        (true, ServiceLifetime.Transient) => $"AddTransient(typeof({ifname}), typeof({name}))",

                        (false, ServiceLifetime.Singleton) => $"AddKeyedSingleton(typeof({ifname}),\"{tag}\", typeof({name}))",
                        (false, ServiceLifetime.Scoped) => $"AddKeyedScoped(typeof({ifname}),\"{tag}\", typeof({name}))",
                        (false, ServiceLifetime.Transient) => $"AddKeyedTransient(typeof({ifname}), \"{tag}\", typeof({name}))",
                        _ => throw new InvalidOperationException() // ここに来ることはない
                    };
                    yield return $"service.{registerFunc};";
                }
            }
        }
    }

    private static void GenerateRegisterMethod(SourceProductionContext context, (INamedTypeSymbol? targetClass, ImmutableArray<INamedTypeSymbol?> registerClasses) target)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        var (targetClass, registerClasses) = target;
        if (targetClass != null)
        {
            var nmspc = targetClass.ContainingNamespace.ToString();
            var clsnm = targetClass.Name;
            var lines = GetRegisterLines(registerClasses);

            var csCode = string.Format(TemplateReader.RegisterCS, nmspc, clsnm, string.Join("\r\n        ", lines));

            context.AddSource($"{clsnm}_Generated.cs", csCode);
        }
    }
}
