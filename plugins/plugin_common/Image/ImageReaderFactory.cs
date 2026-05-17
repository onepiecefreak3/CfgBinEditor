using plugin_common.Image.InternalContract;

namespace plugin_common.Image
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
