namespace plugin_common.Font.DataClasses
{
    public class FontGlyphData
    {
        public ushort FallbackCharacter { get; set; }
        public int MaxHeight { get; set; }
        public IDictionary<ushort, GlyphData> Glyphs { get; set; }
    }
}
