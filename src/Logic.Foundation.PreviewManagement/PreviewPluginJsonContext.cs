using System.Text.Json.Serialization;

namespace Logic.Foundation.PreviewManagement;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(PluginMetadataData))]
internal partial class PreviewPluginJsonContext : JsonSerializerContext;
