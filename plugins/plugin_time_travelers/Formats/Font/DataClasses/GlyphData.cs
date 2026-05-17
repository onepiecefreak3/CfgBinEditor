namespace plugin_time_travelers.Formats.Font.DataClasses
{
    public class GlyphData
    {
        public ushort CodePoint { get; set; }
        public int Width { get; set; }

        public GlyphLocationData Location { get; set; }
        public GlyphDescriptionData Description { get; set; }
    }
}
