using Kaligraphy.Contract.DataClasses.Rendering;
using Kaligraphy.Contract.Rendering;
using plugin_common.Font.DataClasses;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace plugin_common.Font
{
    internal class CtrGlyphProvider(FontImageData fontData, bool isFurigana) : IGlyphProvider
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

            ColorMatrix colorMatrix = CreateColorMatrix(fontGlyph.Location.Index, textColor);

            var srcRect = new Rectangle(fontGlyph.Location.X, fontGlyph.Location.Y, fontGlyph.Description.Width, fontGlyph.Description.Height);

            var glyphImage = fontData.Images[0].Image.GetImage().Clone(context => context.Crop(srcRect).Filter(colorMatrix));

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

        private ColorMatrix CreateColorMatrix(int channelIndex, Color targetColor)
        {
            float sourceR = channelIndex == 0 ? 1f : 0;
            float sourceG = channelIndex == 1 ? 1f : 0;
            float sourceB = channelIndex == 2 ? 1f : 0;

            var pixel = targetColor.ToPixel<Rgba32>();
            float targetR = pixel.R / 255f;
            float targetG = pixel.G / 255f;
            float targetB = pixel.B / 255f;

            return new ColorMatrix(
                0f, 0f, 0f, sourceR,
                0f, 0f, 0f, sourceG,
                0f, 0f, 0f, sourceB,
                0f, 0f, 0f, 0f,
                targetR, targetG, targetB, 0f);
        }
    }
}
