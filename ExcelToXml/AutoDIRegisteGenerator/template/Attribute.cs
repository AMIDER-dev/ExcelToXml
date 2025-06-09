using System;
using Microsoft.Extensions.DependencyInjection;

namespace DIRegisterExtension;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
internal class RegistClassAttribute : Attribute
{
    public ServiceLifetime Lifetime { get; set; }
    public string? Name { get; set; } = null;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
internal class GenerateRegisterAttribute : Attribute
{
}
