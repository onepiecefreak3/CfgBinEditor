using plugin_time_travelers.Formats.Image.InternalContract;

namespace plugin_time_travelers.Formats.Image
{
    internal class ImageReaderFactory
    {
        public static IImageReader Create(int version)
        {
            switch (version)
            {
                case 0:
                    return new Img00Reader();

                default:
                    throw new InvalidOperationException($"Unknown image version {version}.");
            }
        }
    }
}
