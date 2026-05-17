using Kaligraphy.Contract.Rendering;
using plugin_time_travelers.Formats.Font.DataClasses;
using plugin_time_travelers.Formats.Font.Enums;

namespace plugin_time_travelers.Formats.Font
{
    internal class GlyphProviderFactory
    {
        public static IGlyphProvider Create(FontImageData fontData, bool isFurigana)
        {
            switch (fontData.Platform)
            {
                case PlatformType.Ctr:
                    return new CtrGlyphProvider(fontData, isFurigana);

                case PlatformType.Psp:
                    return new PspGlyphProvider(fontData, isFurigana);

                default:
                    throw new InvalidOperationException($"Unsupported platform {fontData.Platform} for glyph providers.");
            }
        }
    }
}
