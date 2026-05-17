using plugin_common.Image.DataClasses;
using plugin_common.Image.InternalContract;

namespace plugin_common.Image
{
    public class ImageParser
    {
        public static ImageData Parse(Stream input)
        {
            int imageVersion = ImageVersionReader.Peek(input);

            IImageReader imageReader = ImageReaderFactory.Create(imageVersion);
            RawImageData imageData = imageReader.Read(input);

            return ImageDecoder.Decode(imageData);
        }
    }
}
