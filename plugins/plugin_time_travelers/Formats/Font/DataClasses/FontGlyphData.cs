using Kaligraphy.Contract.DataClasses;

namespace plugin_time_travelers.Formats.Font.DataClasses
{
    public class FontGlyphData
    {
        public ushort FallbackCharacter { get; set; }
        public int MaxHeight { get; set; }
        public IDictionary<ushort, GlyphData> Glyphs { get; set; }
    }
}
