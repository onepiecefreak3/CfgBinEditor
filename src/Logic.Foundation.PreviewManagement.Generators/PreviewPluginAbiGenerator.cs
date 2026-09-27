using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Logic.Foundation.PreviewManagement.Generators;

[Generator]
public sealed class PreviewPluginAbiGenerator : IIncrementalGenerator
{
    private const string PreviewPluginMetadataName = "Logic.Foundation.PreviewManagement.Abstract.IPreviewPlugin";
    private const string CharacterDataMetadataName = "Kaligraphy.Contract.DataClasses.Parsing.CharacterData";
    private const string Rgba32MetadataName = "SixLabors.ImageSharp.PixelFormats.Rgba32";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<GenerationResult> result = context.CompilationProvider
            .Select(static (compilation, _) => Create(compilation));

        context.RegisterSourceOutput(result, static (source, generation) =>
        {
            foreach (DiagnosticInfo info in generation.Diagnostics)
                source.ReportDiagnostic(CreateDiagnostic(info));

            if (generation.Source is not null)
            {
                source.AddSource(
                    "PreviewPluginAbi.g.cs",
                    SourceText.From(generation.Source, Encoding.UTF8));
            }
        });
    }

    private static GenerationResult Create(Compilation compilation)
    {
        INamedTypeSymbol? previewPlugin = compilation.GetTypeByMetadataName(PreviewPluginMetadataName);
        INamedTypeSymbol? characterData = compilation.GetTypeByMetadataName(CharacterDataMetadataName);
        if (previewPlugin is null || characterData is null)
            return new GenerationResult(null, ImmutableArray<DiagnosticInfo>.Empty);

        var diagnostics = new List<DiagnosticInfo>();
        List<PluginModel> plugins = CollectPlugins(compilation, previewPlugin, diagnostics);
        List<CharacterTypeModel> characters = CollectCharacters(compilation, characterData, diagnostics);

        if (plugins.Count == 0)
            return new GenerationResult(null, diagnostics.ToImmutableArray());

        if (diagnostics.Exists(diagnostic => diagnostic.Id is "CBE001" or "CBE002" or "CBE003"))
            return new GenerationResult(null, diagnostics.ToImmutableArray());

        string source = PreviewPluginAbiEmitter.Emit(plugins, characters);
        return new GenerationResult(source, diagnostics.ToImmutableArray());
    }

    private static List<PluginModel> CollectPlugins(
        Compilation compilation,
        INamedTypeSymbol previewPlugin,
        List<DiagnosticInfo> diagnostics)
    {
        var plugins = new List<PluginModel>();
        foreach (INamedTypeSymbol type in EnumerateTypes(compilation.Assembly))
        {
            if (type.TypeKind != TypeKind.Class || type.IsAbstract || type.IsStatic)
                continue;

            if (!Implements(type, previewPlugin))
                continue;

            if (!HasAccessibleConstructor(type))
            {
                diagnostics.Add(Describe(Diagnostics.MissingPluginConstructor, type.Locations, type.Name));
                continue;
            }

            (Guid pluginId, bool hasPluginId) = ReadPluginId(type);
            plugins.Add(new PluginModel(
                type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                pluginId,
                hasPluginId));
        }

        plugins.Sort(static (left, right) =>
        {
            int group = left.HasPluginId.CompareTo(right.HasPluginId);
            if (group != 0)
                return -group;

            int id = left.PluginId.CompareTo(right.PluginId);
            return id != 0 ? id : string.CompareOrdinal(left.QualifiedName, right.QualifiedName);
        });

        return plugins;
    }

    private static List<CharacterTypeModel> CollectCharacters(
        Compilation compilation,
        INamedTypeSymbol characterData,
        List<DiagnosticInfo> diagnostics)
    {
        var characters = new List<CharacterTypeModel>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (INamedTypeSymbol type in EnumerateTypes(compilation.Assembly))
        {
            if (!IsConcreteCharacter(type, characterData) || type.DeclaringSyntaxReferences.Length == 0)
                continue;

            if (type.DeclaredAccessibility is not (Accessibility.Public or Accessibility.Internal))
                continue;

            if (TryCreateCharacter(type, characterData, isSource: true, diagnostics) is { } model && seen.Add(model.MetadataName))
                characters.Add(model);
        }

        foreach (IAssemblySymbol assembly in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            if (!References(assembly, characterData.ContainingAssembly) &&
                !SymbolEqualityComparer.Default.Equals(assembly, characterData.ContainingAssembly))
                continue;

            foreach (INamedTypeSymbol type in EnumerateTypes(assembly))
            {
                if (!IsConcreteCharacter(type, characterData) || !IsPublicType(type))
                    continue;

                if (!type.Constructors.Any(constructor => constructor.Parameters.Length == 0 &&
                                                          constructor.DeclaredAccessibility == Accessibility.Public))
                    continue;

                if (TryCreateCharacter(type, characterData, isSource: false, diagnostics) is { } model &&
                    seen.Add(model.MetadataName))
                    characters.Add(model);
            }
        }

        ReportCollisions(characters, diagnostics);
        characters.Sort(static (left, right) =>
        {
            int depth = right.Depth.CompareTo(left.Depth);
            return depth != 0 ? depth : string.CompareOrdinal(left.MetadataName, right.MetadataName);
        });

        return characters;
    }

    private static CharacterTypeModel? TryCreateCharacter(
        INamedTypeSymbol type,
        INamedTypeSymbol characterData,
        bool isSource,
        List<DiagnosticInfo> diagnostics)
    {
        var properties = new List<PropertyModel>();
        foreach (IPropertySymbol property in PayloadProperties(type, characterData))
        {
            if (!IsSetterAccessible(property, isSource))
                continue;

            if (!TryGetKind(property.Type, out WireKind kind, out bool isEnum))
            {
                if (isSource)
                {
                    diagnostics.Add(Describe(
                        Diagnostics.UnsupportedProperty,
                        property.Locations,
                        property.Name,
                        type.Name,
                        property.Type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat)));
                }
                else
                    return null;

                continue;
            }

            properties.Add(new PropertyModel(
                property.Name,
                property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                kind,
                isEnum));
        }

        string metadataName = MetadataName(type);
        return new CharacterTypeModel(
            type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            metadataName,
            Fnv1A(metadataName),
            Depth(type, characterData),
            properties.ToImmutableArray());
    }

    private static void ReportCollisions(List<CharacterTypeModel> characters, List<DiagnosticInfo> diagnostics)
    {
        var owners = new Dictionary<uint, string>();
        foreach (CharacterTypeModel character in characters)
        {
            if (owners.TryGetValue(character.TypeId, out string? other))
            {
                diagnostics.Add(new DiagnosticInfo(
                    Diagnostics.TypeIdCollision.Id,
                    string.Format(
                        Diagnostics.TypeIdCollision.MessageFormat.ToString(),
                        character.TypeId,
                        other,
                        character.MetadataName),
                    null,
                    0,
                    0));
            }
            else
                owners.Add(character.TypeId, character.MetadataName);
        }
    }

    private static IEnumerable<IPropertySymbol> PayloadProperties(
        INamedTypeSymbol type,
        INamedTypeSymbol characterData)
    {
        var chain = new List<INamedTypeSymbol>();
        for (INamedTypeSymbol? current = type;
             current is not null && !SymbolEqualityComparer.Default.Equals(current, characterData);
             current = current.BaseType)
            chain.Add(current);

        chain.Reverse();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (INamedTypeSymbol declaredOn in chain)
        {
            foreach (IPropertySymbol property in declaredOn.GetMembers().OfType<IPropertySymbol>())
            {
                if (property.IsStatic || property.Parameters.Length > 0)
                    continue;

                if (property.GetMethod is null || property.SetMethod is null || property.OverriddenProperty is not null)
                    continue;

                if (property.Name is "IsVisible" or "IsPersistent")
                    continue;

                if (seen.Add(property.Name))
                    yield return property;
            }
        }
    }

    private static bool TryGetKind(ITypeSymbol type, out WireKind kind, out bool isEnum)
    {
        isEnum = false;
        if (type is INamedTypeSymbol named && named.TypeKind == TypeKind.Enum && named.EnumUnderlyingType is not null)
        {
            isEnum = true;
            return TryGetKind(named.EnumUnderlyingType, out kind, out _);
        }

        if (type is INamedTypeSymbol rgba &&
            rgba.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::" + Rgba32MetadataName)
        {
            kind = WireKind.Rgba32;
            return true;
        }

        kind = type.SpecialType switch
        {
            SpecialType.System_Boolean => WireKind.Bool,
            SpecialType.System_Byte => WireKind.UInt8,
            SpecialType.System_SByte => WireKind.Int8,
            SpecialType.System_Int16 => WireKind.Int16,
            SpecialType.System_UInt16 => WireKind.UInt16,
            SpecialType.System_Int32 => WireKind.Int32,
            SpecialType.System_UInt32 => WireKind.UInt32,
            SpecialType.System_Int64 => WireKind.Int64,
            SpecialType.System_UInt64 => WireKind.UInt64,
            SpecialType.System_Char => WireKind.Char,
            SpecialType.System_String => WireKind.String,
            SpecialType.System_Single => WireKind.Single,
            SpecialType.System_Double => WireKind.Double,
            _ => (WireKind)(-1)
        };

        return Enum.IsDefined(typeof(WireKind), kind);
    }

    private static bool IsSetterAccessible(IPropertySymbol property, bool isSource)
    {
        Accessibility accessibility = property.SetMethod!.DeclaredAccessibility;
        if (accessibility == Accessibility.Public)
            return true;

        return isSource && accessibility == Accessibility.Internal;
    }

    private static bool IsConcreteCharacter(INamedTypeSymbol type, INamedTypeSymbol characterData)
    {
        if (type.TypeKind != TypeKind.Class || type.IsAbstract || type.IsStatic || type.Name.Contains('<'))
            return false;

        for (INamedTypeSymbol? current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, characterData))
                return true;
        }

        return false;
    }

    private static bool IsPublicType(INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? current = type; current is not null; current = current.ContainingType)
        {
            if (current.DeclaredAccessibility != Accessibility.Public)
                return false;
        }

        return true;
    }

    private static bool HasAccessibleConstructor(INamedTypeSymbol type)
    {
        return type.Constructors.Any(constructor =>
            constructor.Parameters.Length == 0 &&
            constructor.DeclaredAccessibility is Accessibility.Public or Accessibility.Internal);
    }

    private static bool Implements(INamedTypeSymbol type, INamedTypeSymbol iface)
    {
        foreach (INamedTypeSymbol implemented in type.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(implemented, iface))
                return true;
        }

        return false;
    }

    private static int Depth(INamedTypeSymbol type, INamedTypeSymbol characterData)
    {
        int depth = 0;
        for (INamedTypeSymbol? current = type;
             current is not null && !SymbolEqualityComparer.Default.Equals(current, characterData);
             current = current.BaseType)
            depth++;

        return depth;
    }

    private static (Guid Id, bool HasId) ReadPluginId(INamedTypeSymbol type)
    {
        IPropertySymbol? property = type.GetMembers("PluginId").OfType<IPropertySymbol>().FirstOrDefault();
        if (property is null)
            return (Guid.Empty, false);

        foreach (SyntaxReference syntax in property.DeclaringSyntaxReferences)
        {
            Match match = Regex.Match(
                syntax.GetSyntax().ToString(),
                "Guid\\.Parse\\(\\s*\"([0-9a-fA-F-]+)\"\\s*\\)");

            if (match.Success && Guid.TryParse(match.Groups[1].Value, out Guid pluginId))
                return (pluginId, true);
        }

        return (Guid.Empty, false);
    }

    private static string MetadataName(INamedTypeSymbol type)
    {
        return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Replace("global::", "");
    }

    private static uint Fnv1A(string text)
    {
        uint hash = 2166136261;
        foreach (byte value in Encoding.UTF8.GetBytes(text))
        {
            hash ^= value;
            hash *= 16777619;
        }

        return hash;
    }

    private static bool References(IAssemblySymbol assembly, IAssemblySymbol target)
    {
        foreach (IModuleSymbol module in assembly.Modules)
        {
            foreach (IAssemblySymbol reference in module.ReferencedAssemblySymbols)
            {
                if (SymbolEqualityComparer.Default.Equals(reference, target))
                    return true;
            }
        }

        return false;
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateTypes(IAssemblySymbol assembly)
    {
        var stack = new Stack<INamespaceOrTypeSymbol>();
        stack.Push(assembly.GlobalNamespace);
        while (stack.Count > 0)
        {
            INamespaceOrTypeSymbol current = stack.Pop();
            if (current is INamedTypeSymbol type)
            {
                yield return type;
                foreach (INamedTypeSymbol nested in type.GetTypeMembers())
                    stack.Push(nested);

                continue;
            }

            if (current is INamespaceSymbol ns)
            {
                foreach (INamespaceOrTypeSymbol member in ns.GetMembers())
                    stack.Push(member);
            }
        }
    }

    private static DiagnosticInfo Describe(DiagnosticDescriptor descriptor, IEnumerable<Location> locations, params object[] args)
    {
        Location? location = locations.FirstOrDefault();
        FileLinePositionSpan span = location?.GetLineSpan() ?? default;
        string message = string.Format(descriptor.MessageFormat.ToString(), args);
        return new DiagnosticInfo(
            descriptor.Id,
            message,
            span.Path,
            span.StartLinePosition.Line + 1,
            span.StartLinePosition.Character + 1);
    }

    private static Diagnostic CreateDiagnostic(DiagnosticInfo info)
    {
        DiagnosticDescriptor template = info.Id switch
        {
            "CBE001" => Diagnostics.MissingPluginConstructor,
            "CBE002" => Diagnostics.UnsupportedProperty,
            _ => Diagnostics.TypeIdCollision
        };

        var descriptor = new DiagnosticDescriptor(
            template.Id,
            template.Title,
            "{0}",
            template.Category,
            template.DefaultSeverity,
            isEnabledByDefault: true);

        Location location = string.IsNullOrEmpty(info.Path)
            ? Location.None
            : Location.Create(
                info.Path!,
                default,
                new LinePositionSpan(
                    new LinePosition(Math.Max(info.Line - 1, 0), Math.Max(info.Column - 1, 0)),
                    new LinePosition(Math.Max(info.Line - 1, 0), Math.Max(info.Column - 1, 0))));

        return Diagnostic.Create(descriptor, location, info.Message);
    }
}
