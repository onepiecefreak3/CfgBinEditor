using System.Text.Json;
using Kaligraphy.Contract.DataClasses.Parsing;
using Kaligraphy.Contract.Parsing;
using Konnect.Contract.DataClasses.Plugin;
using Konnect.Contract.Management.Assembly;
using Logic.Foundation.PreviewManagement.Abstract;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Logic.Foundation.PreviewManagement;

internal sealed class NativePreviewPlugin : IPreviewPlugin
{
    private readonly NativePreviewLibrary _library;
    private readonly int _index;

    public NativePreviewPlugin(NativePreviewLibrary library, int index)
    {
        _library = library;
        _index = index;
        PluginId = library.GetId(index);
        Metadata = ReadMetadata(library.GetMetadata(index));
        Deserializer = new AbiDeserializer(library, index);
    }

    public Guid PluginId { get; }

    public PluginMetadata Metadata { get; }

    public ICharacterDeserializer? Deserializer { get; }

    public Task<Image<Rgba32>?> RenderPreview(IList<CharacterData> characters)
    {
        byte[] wire = CharacterWireEnvelope.Write(characters);
        byte[] encoded = _library.Render(_index, wire);
        return Task.FromResult(DecodeImage(encoded));
    }

    public void RegisterAssemblies(IAssemblyManager manager)
    {
    }

    private static PluginMetadata ReadMetadata(byte[] json)
    {
        PluginMetadataData? data = JsonSerializer.Deserialize(json, PreviewPluginJsonContext.Default.PluginMetadataData);
        return new PluginMetadata
        {
            Name = data?.Name ?? string.Empty,
            Author = data?.Author ?? [],
            Platform = data?.Platform ?? [],
            Developer = data?.Developer ?? string.Empty,
            Publisher = data?.Publisher ?? string.Empty,
            LongDescription = data?.LongDescription ?? string.Empty,
            ShortDescription = data?.ShortDescription ?? string.Empty,
            Website = data?.Website ?? string.Empty
        };
    }

    private static Image<Rgba32>? DecodeImage(byte[] encoded)
    {
        if (encoded.Length == 0)
            return null;

        if (encoded.Length < 8)
            throw new InvalidOperationException("Native preview image is truncated.");

        int width = BitConverter.ToInt32(encoded, 0);
        int height = BitConverter.ToInt32(encoded, 4);
        if (width <= 0 || height <= 0)
            return null;

        long byteCount = (long)width * height * 4;
        if (encoded.Length != 8 + byteCount)
            throw new InvalidOperationException("Native preview image size does not match its pixels.");

        var image = new Image<Rgba32>(width, height);
        int offset = 8;
        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < height; y++)
            {
                Span<Rgba32> row = accessor.GetRowSpan(y);
                for (int x = 0; x < width; x++)
                {
                    row[x] = new Rgba32(encoded[offset], encoded[offset + 1], encoded[offset + 2], encoded[offset + 3]);
                    offset += 4;
                }
            }
        });

        return image;
    }

    private sealed class AbiDeserializer(NativePreviewLibrary library, int index) : ICharacterDeserializer
    {
        public IList<CharacterData> Deserialize(string text)
        {
            byte[] wire = library.Deserialize(index, text);
            return CharacterWireEnvelope.Read(wire);
        }
    }
}
