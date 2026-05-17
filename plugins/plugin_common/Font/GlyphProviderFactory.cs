using Kaligraphy.Contract.Rendering;
using plugin_common.Font.DataClasses;
using plugin_common.Font.Enums;

namespace plugin_common.Font
{
    public class GlyphProviderFactory
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
