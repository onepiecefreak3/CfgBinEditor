using plugin_time_travelers.Formats.Font.InternalContract;

namespace plugin_time_travelers.Formats.Font
{
    internal class FontReaderFactory
    {
        public static IFontReader Create(int version)
        {
            switch (version)
            {
                case 0:
                    return new Fnt00Reader();

                case 1:
                    return new Fnt01Reader();
                
                default:
                    throw new InvalidOperationException($"Unknown font version {version}.");
            }
        }
    }
}
