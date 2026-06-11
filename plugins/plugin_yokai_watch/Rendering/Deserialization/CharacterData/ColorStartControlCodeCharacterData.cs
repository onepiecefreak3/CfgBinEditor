using Kaligraphy.Contract.DataClasses.Parsing;
using SixLabors.ImageSharp.PixelFormats;

namespace plugin_yokai_watch.Rendering.Deserialization.CharacterData
{
    class ColorStartControlCodeCharacterData : ControlCodeCharacterData
    {
        public required Rgba32 Color { get; init; }
    }
}
