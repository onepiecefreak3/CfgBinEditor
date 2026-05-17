using Kaligraphy.Contract.DataClasses.Rendering;
using Kaligraphy.Contract.Rendering;
using plugin_time_travelers.Formats.Font.DataClasses;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace plugin_time_travelers.Formats.Font
{
    internal class PspGlyphProvider(FontImageData fontData, bool isFurigana) : IGlyphProvider
    {
        public CharacterInfo? GetOrDefault(ushort codePoint)
        {
            return GetOrDefault(codePoint, Color.White);
        }

        public CharacterInfo? GetOrDefault(ushort codePoint, Color textColor)
        {
            GlyphData? fontGlyph = GetGlyph(codePoint);
            if (fontGlyph == null)
                return null;

            if (fontGlyph.Description.Width == 0 || fontGlyph.Description.Height == 0)
                return null;

            var srcRect = new Rectangle(fontGlyph.Location.X, fontGlyph.Location.Y, fontGlyph.Description.Width, fontGlyph.Description.Height);

            var glyphImage = fontData.Images[fontGlyph.Location.Index].Image.GetImage().Clone(context => context.Crop(srcRect));

            return new CharacterInfo
            {
                CodePoint = (char)codePoint,
                Glyph = glyphImage,
                BoundingBox = new Size(fontGlyph.Description.Width, fontGlyph.Description.Height),
                GlyphPosition = new Point(fontGlyph.Description.X, fontGlyph.Description.Y)
            };
        }

        public int GetMaxHeight() => isFurigana ? fontData.Font.SmallFont.MaxHeight : fontData.Font.LargeFont.MaxHeight;

        public GlyphData? GetGlyph(ushort character)
        {
            FontGlyphData fontGlyphs = isFurigana ?
                fontData.Font.SmallFont :
                fontData.Font.LargeFont;

            if (fontGlyphs.Glyphs.TryGetValue(character, out GlyphData? glyph))
                return glyph;

            if (fontGlyphs.Glyphs.TryGetValue(fontGlyphs.FallbackCharacter, out glyph))
                return glyph;

            return null;
        }
    }
}
