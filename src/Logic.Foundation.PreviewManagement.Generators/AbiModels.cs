using System.Collections.Immutable;

namespace Logic.Foundation.PreviewManagement.Generators;

internal enum WireKind
{
    Bool,
    UInt8,
    Int8,
    Int16,
    UInt16,
    Int32,
    UInt32,
    Int64,
    UInt64,
    Char,
    String,
    Single,
    Double,
    Rgba32
}

internal sealed record PropertyModel(
    string Name,
    string QualifiedType,
    WireKind Kind,
    bool IsEnum);

internal sealed record CharacterTypeModel(
    string QualifiedName,
    string MetadataName,
    uint TypeId,
    int Depth,
    ImmutableArray<PropertyModel> Properties);

internal sealed record PluginModel(string QualifiedName, Guid PluginId, bool HasPluginId);

internal sealed record GenerationResult(
    string? Source,
    ImmutableArray<DiagnosticInfo> Diagnostics);

internal sealed record DiagnosticInfo(
    string Id,
    string Message,
    string? Path,
    int Line,
    int Column);
