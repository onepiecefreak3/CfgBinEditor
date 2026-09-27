using Microsoft.CodeAnalysis;

namespace Logic.Foundation.PreviewManagement.Generators;

internal static class Diagnostics
{
    public static readonly DiagnosticDescriptor MissingPluginConstructor = new(
        id: "CBE001",
        title: "Preview plugin needs a parameterless constructor",
        messageFormat: "Preview plugin '{0}' must declare an accessible parameterless constructor",
        category: "PreviewPluginAbi",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnsupportedProperty = new(
        id: "CBE002",
        title: "CharacterData property cannot be written to the wire",
        messageFormat: "Property '{0}' on '{1}' has unsupported type '{2}'",
        category: "PreviewPluginAbi",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TypeIdCollision = new(
        id: "CBE003",
        title: "CharacterData wire type id collision",
        messageFormat: "Character type id 0x{0:X8} collides between '{1}' and '{2}'",
        category: "PreviewPluginAbi",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
